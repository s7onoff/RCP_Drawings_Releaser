using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
                VM.FieldResults = new BindingList<StringLogic.ResultField>(StringLogic.AnalyzeData(
                        VM.NewDrawings.Select(file => file.FullName).ToList().
                            Concat(VM.OldDrawings.Select(file => file.FullName).ToList()).
                            ToList())
                        );
            }
        }
        #endregion

        #region Props

        public AnalyzeCommand Analyze { get; set; }

        public BindingList<StringLogic.ResultField> FieldResults
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
        private BindingList<StringLogic.ResultField> _fieldResults;

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
                    TableItemsFromDirectory(Directory.GetFiles(item), ref drawingsList);
                }
            }
        }

        #endregion Methods

        #region Classes

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
            FieldResults = new BindingList<StringLogic.ResultField>();
        }

        #endregion
    }

}
