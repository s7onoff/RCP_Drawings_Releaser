using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using RCP_Drawings_Releaser.Annotations;


namespace RCP_Drawings_Releaser.ViewModels
{
    internal class MainWindowVM : INotifyPropertyChanged
    {
        #region INotify
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Commands
        
        public abstract class ButtonCommand : ICommand
        {
            public MainWindowVM VM;

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            public bool CanExecute(object parameter)
            {
                var buttonsEnabled = true;
                if (parameter != null)
                {
                    buttonsEnabled = (bool)parameter;
                }

                return buttonsEnabled;
            }

            public abstract void Execute(object parameter);

            public ButtonCommand(MainWindowVM vm)
            {
                VM = vm;
            }
        }
        
        
        public class AnalyzeCommand : ButtonCommand
        {
            public AnalyzeCommand(MainWindowVM vm) : base(vm) { }

            public override void Execute(object parameter)
            {
                VM.FieldResults.Clear();
                var results = new List<StringLogic.FieldColumn>(StringLogic.AnalyzeData(
                        VM.NewDrawings.Select(file => file.FullName).ToList().
                            Concat(VM.OldDrawings.Select(file => file.FullName).ToList()).
                            ToList())
                        );
                for (var index = 0; index < results.Count; index++)
                {
                    var column = results[index];
                    var thisFieldResult = new ResultField();

                    if (index > 0 && column.ColumnSide == Side.Right && results[index - 1].ColumnSide == Side.Left)
                    {
                        VM.FieldResults.Add(new ResultField()
                        {
                            Value = "<....>"
                        });
                    }
                    
                    if (column.IsConst)
                    {
                        thisFieldResult.Value = column.ConstValue;
                    }

                    thisFieldResult.IsSheetNum = column.IsSheetNum;
                    thisFieldResult.IsRevNum = column.IsRevNum;
                    thisFieldResult.AllValues = column.Fields.ToArray();
                    thisFieldResult.ResultSide = column.ColumnSide;
                    thisFieldResult.Num = column.Num;

                    VM.FieldResults.Add(thisFieldResult);
                }
            }
        }
        
        #endregion

        #region Props
        
        public AnalyzeCommand Analyze { get; set; }
        
        private BindingList<ResultField> _fieldResults;
        public BindingList<ResultField> FieldResults
        {
            get => _fieldResults;
            set
            {
                _fieldResults = value; 
                OnPropertyChanged(nameof(FieldResults));
            }
        }

        private List<ImportedFile> _newDrawings;
        public List<ImportedFile> NewDrawings
        {
            get => _newDrawings;
            set
            {
                _newDrawings = value;
                OnPropertyChanged(nameof(NewDrawings));
            }
        }

        private List<ImportedFile> _oldDrawings;
        public List<ImportedFile> OldDrawings
        {
            get => _oldDrawings;
            set
            {
                _oldDrawings = value;
                OnPropertyChanged(nameof(OldDrawings));
            }
        }

        #endregion

        #region Methods

        public void FillFileTable(string[] files, string tableName)
        {
            var tableChooser = new Dictionary<string, List<ImportedFile>>()
            {
                {"NewDrawingsGrid", NewDrawings },
                {"OldDrawingsGrid", OldDrawings }
            };

            var chosenTable = tableChooser[tableName];

            TableItemsFromDirectory(files, ref chosenTable);
        }

        private ImportedFile TableItemFromFile(string file)
        {
            // var drawingFileString = new StringLogic.FileString(file);
            
            var drawing = new ImportedFile
            {
                FullPath = Path.Combine(file),
                Name = Path.GetFileNameWithoutExtension(file),
                Extension = Path.GetExtension(file),
                FullName = Path.GetFileName(file)
            };

            return drawing;
        }

        private void TableItemsFromDirectory(string[] files, ref List<ImportedFile> drawingsList)
        {
            foreach (var item in files)
            {
                if (File.Exists(item))
                {
                    drawingsList.Add(TableItemFromFile(item));
                }
                else if (Directory.Exists(item))
                {
                    TableItemsFromDirectory(Directory.GetFiles(item).Concat(Directory.GetDirectories(item)).ToArray(), ref drawingsList);
                }
            }
        }
        
        public void ChangeListNumField(ResultField newListField)
        {
            foreach (var field in FieldResults)
            {
                field.IsSheetNum = false;
            }
            newListField.IsSheetNum = true;
            OnPropertyChanged(nameof(FieldResults));
            //todo:not working update of converter binding:
            OnPropertyChanged(nameof(newListField));
            OnPropertyChanged(nameof(newListField.IsSheetNum));
        }

        #endregion Methods

        #region Classes

        public class ResultField
        {
            
            public string Value { get; set; }
            public bool IsSheetNum { get; set; }
            public bool IsRevNum { get; set; }
            public string[] AllValues { get; set; }
            public Side ResultSide { get; set; }
            public int Num { get; set; }

            public ResultField()
            {
                Value = "xx";
                IsRevNum = false;
                IsSheetNum = false;
                AllValues = new string[]{};
                ResultSide = Side.Left;
                Num = 0;
            }
        }
        
        public sealed class ImportedFile : INotifyPropertyChanged
        {
            #region Props

            private string _name;

            public string Name
            {
                get => _name;
                set
                {
                    _name = value; 
                    OnDrawingPropertyChanged(nameof(Name));
                }
            }

            private string _extension;

            public string Extension
            {
                get => _extension;
                set
                {
                    _extension = value; 
                    OnDrawingPropertyChanged(nameof(Extension));
                }
            }

            public string FullName { get; set; }
            
            private string _fullPath;

            public string FullPath
            {
                get => _fullPath;
                set
                {
                    _fullPath = value; 
                    OnDrawingPropertyChanged(nameof(FullPath));
                }
            }

            #endregion

            #region INotify
            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            private void OnDrawingPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }

        #endregion

        #region Ctor

        public MainWindowVM()
        {
            NewDrawings = new List<ImportedFile>();
            OldDrawings = new List<ImportedFile>();
            Analyze = new AnalyzeCommand(this);
            FieldResults = new BindingList<ResultField>();
        }

        #endregion

        
    }

}
