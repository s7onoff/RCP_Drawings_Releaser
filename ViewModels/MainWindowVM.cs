using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
                    thisFieldResult.NumFromRight = column.NumFromRight;

                    VM.FieldResults.Add(thisFieldResult);
                }
            }
        }

        public class SheetNumSelectCommand : ButtonCommand
        {
            public SheetNumSelectCommand(MainWindowVM vm) : base(vm) { }

            public override void Execute(object parameter)
            {
                if (VM.SheetNumSelectingEnabled)
                {
                    VM.SheetNumSelectingEnabled = false;
                }
                else
                {
                    VM.RevNumSelectingEnabled = false;
                    VM.SheetNumSelectingEnabled = true;   
                }
            }
        }
        
        public class RevNumSelectCommand : ButtonCommand
        {
            public RevNumSelectCommand(MainWindowVM vm) : base(vm) { }

            public override void Execute(object parameter)
            {
                if (VM.RevNumSelectingEnabled)
                {
                    VM.RevNumSelectingEnabled = false;
                }
                else
                {
                    VM.SheetNumSelectingEnabled = false;
                    VM.RevNumSelectingEnabled = true;      
                }
            }
        }
        
        public class FillAlbumListCommand : ButtonCommand
        {
            public FillAlbumListCommand(MainWindowVM vm) : base(vm) { }

            public override void Execute(object parameter)
            {
                VM.FillAlbumDrawings();
            }
        }
        #endregion

        #region Props
        
        public bool SheetNumSelectingEnabled
        {
            get => _sheetNumSelectingEnabled;
            set
            {
                _sheetNumSelectingEnabled = value;
                OnPropertyChanged(nameof(SheetNumSelectingEnabled));
            }
        }
        
        public bool RevNumSelectingEnabled
        {
            get => _revNumSelectingEnabled;
            set
            {
                _revNumSelectingEnabled = value;
                OnPropertyChanged(nameof(RevNumSelectingEnabled));
            }
        }

        public string Delimiters
        {
            get => _delimiters;
            set
            {
                _delimiters = value; 
                OnPropertyChanged(nameof(Delimiters));
                StringLogic.ChangeDelimiters(value);
            }
        }

        public AnalyzeCommand Analyze { get; }
        public SheetNumSelectCommand SheetNumSelect { get; }
        public RevNumSelectCommand RevNumSelect { get;  }
        public FillAlbumListCommand FillAlbumList { get; }
        
        public BindingList<ResultField> FieldResults
        {
            get => _fieldResults;
            set
            {
                _fieldResults = value; 
                OnPropertyChanged(nameof(FieldResults));
            }
        }

        public List<ImportedFile> NewDrawings
        {
            get => _newDrawings;
            set
            {
                _newDrawings = value;
                OnPropertyChanged(nameof(NewDrawings));
            }
        }


        public List<ImportedFile> OldDrawings
        {
            get => _oldDrawings;
            set
            {
                _oldDrawings = value;
                OnPropertyChanged(nameof(OldDrawings));
            }
        }

        public ObservableCollection<ImportedFile> AlbumDrawings
        {
            get => _albumDrawings;
            set
            {
                _albumDrawings = value; 
                OnPropertyChanged(nameof(AlbumDrawings));
            }
        }

        private bool _sheetNumSelectingEnabled;
        private bool _revNumSelectingEnabled;
        private string _delimiters;
        private BindingList<ResultField> _fieldResults;
        private List<ImportedFile> _newDrawings;
        private List<ImportedFile> _oldDrawings;
        private ObservableCollection<ImportedFile> _albumDrawings;

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
        }
        
        public void ChangeRevNumField(ResultField newListField)
        {
            foreach (var field in FieldResults)
            {
                field.IsRevNum = false;
            }
            newListField.IsRevNum = true;
        }
        
        public void SetDrawingsSheetsAndRevs(List<ImportedFile> drawingsList)
        {
            int sheetPosition = 0;
            Side sheetSide = Side.Left;
            int revPosition = 0;
            Side revSide = Side.Left;

            foreach (var fieldResult in FieldResults)
            {
                if (fieldResult.IsSheetNum)
                {
                    sheetPosition = (fieldResult.ResultSide == Side.Left) ? fieldResult.Num : fieldResult.NumFromRight;
                    sheetSide = fieldResult.ResultSide;
                }
                else if(fieldResult.IsRevNum)
                {
                    revPosition = (fieldResult.ResultSide == Side.Left) ? fieldResult.Num : fieldResult.NumFromRight;
                    revSide = fieldResult.ResultSide;
                }
            }
            
            foreach (var importedFile in drawingsList)
            {
                importedFile.SheetNum = StringLogic.GetNumberField(importedFile.Name, sheetPosition, sheetSide);
                importedFile.RevNum = StringLogic.GetNumberField(importedFile.Name, revPosition, revSide);
            }
        }

        private void FillAlbumDrawings()
        {
            SetDrawingsSheetsAndRevs(NewDrawings);
            SetDrawingsSheetsAndRevs(OldDrawings);
            
            AlbumDrawings.Clear();

            foreach (var drawing in NewDrawings.Where(drawing => drawing.Extension == ".pdf" || drawing.Extension == ".docx"))
            {
                AlbumDrawings.Add(drawing);
            }

            foreach (var oldDrawing in OldDrawings.Where(d => d.Extension == ".pdf" || d.Extension == ".docx"))
            {
                if (AlbumDrawings.Any(albumDrawing => albumDrawing.SheetNum == oldDrawing.SheetNum &&
                                                      albumDrawing.Extension == oldDrawing.Extension &&
                                                      albumDrawing.RevNum < oldDrawing.RevNum))
                {
                    // Вах какой костыль блять
                    var tempList = AlbumDrawings.ToList();
                    AlbumDrawings.Remove(tempList
                        .Find(albumDrawing =>
                            albumDrawing.SheetNum == oldDrawing.SheetNum &&
                            albumDrawing.Extension == oldDrawing.Extension &&
                            albumDrawing.RevNum < oldDrawing.RevNum));
                    AlbumDrawings.Add(oldDrawing);
                    continue;
                }

                if (AlbumDrawings.All(albumDrawing => 
                    (albumDrawing.SheetNum != oldDrawing.SheetNum) || 
                    (albumDrawing.SheetNum == oldDrawing.SheetNum &&
                     albumDrawing.Extension != oldDrawing.Extension)))
                {
                    AlbumDrawings.Add(oldDrawing);
                }
            }

            var orderedDrawings = AlbumDrawings.OrderBy(x => x.SheetNum).ToList();
            AlbumDrawings.Clear();
            foreach (var drawing in orderedDrawings)
            {
                AlbumDrawings.Add(drawing);
            }
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
            public int NumFromRight { get; set; }

            public ResultField()
            {
                Value = "xx";
                IsRevNum = false;
                IsSheetNum = false;
                AllValues = new string[]{};
                ResultSide = Side.Left;
                Num = 0;
                NumFromRight = 0;
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

            public int SheetNum { get; set; }
            public int RevNum { get; set; }

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
            RevNumSelectingEnabled = false;
            SheetNumSelectingEnabled = false;
            Delimiters = ".-_[]";
            NewDrawings = new List<ImportedFile>();
            OldDrawings = new List<ImportedFile>();
            FieldResults = new BindingList<ResultField>();
            AlbumDrawings = new ObservableCollection<ImportedFile>();
            
            Analyze = new AnalyzeCommand(this);
            SheetNumSelect = new SheetNumSelectCommand(this);
            RevNumSelect = new RevNumSelectCommand(this);
            FillAlbumList = new FillAlbumListCommand(this);
        }

        #endregion

        
    }

}
