using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
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
                VM.FillFieldResults();
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
        public class CloseAppCommand : ButtonCommand
        {
            public CloseAppCommand(MainWindowVM vm) : base(vm) { }

            public override void Execute(object parameter)
            {
                Application.Current.Shutdown();
            }
        }

        public class SelectSavingFolderCommand : ButtonCommand
        {
            public SelectSavingFolderCommand(MainWindowVM vm) : base(vm) { }

            public override void Execute(object parameter)
            {
                var dlg = new CommonOpenFileDialog();
                dlg.Title = "Folder to save drawings";
                dlg.IsFolderPicker = true;
                
                try
                {
                    dlg.InitialDirectory = Path.Combine(VM.PathToSave);
                }
                catch (ArgumentException)
                {
                    dlg.InitialDirectory = VM.NewFiles.Count == 0 ? 
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : 
                        Path.Combine(VM.NewFiles[0].FullPath);
                }
                
                dlg.AddToMostRecentlyUsedList = false;
                dlg.AllowNonFileSystemItems = false;
                dlg.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                dlg.EnsureFileExists = true;
                dlg.EnsurePathExists = true;
                dlg.EnsureReadOnly = false;
                dlg.EnsureValidNames = true;
                dlg.Multiselect = false;
                dlg.ShowPlacesList = true;

                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    VM.PathToSave = dlg.FileName;
                }
            }
        }

        public class CreateAlbumPdfCommand : ButtonCommand
        {
            public CreateAlbumPdfCommand(MainWindowVM vm) : base(vm) { }

            public override void Execute(object parameter)
            {
                PDFLogic.MergeFiles(VM.AlbumDrawings.Select(f => 
                    f.FullPath.ToString()).ToList(), VM.PathToSave, string.Concat(VM.PdfName, ".pdf"));
            }
        }
        
        public class FilesToFoldersCommand : ButtonCommand
        {
            public FilesToFoldersCommand(MainWindowVM vm) : base(vm) { }

            public override void Execute(object parameter)
            {
                if (VM.PathToSaveAuto)
                {
                    VM.SelectSavingFolder.Execute(null);
                }
                VM.CreateFolderStructure();
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

        public List<string> DrawingExtensions;

        private bool RevisionsExist { get; set; }
        private bool PathToSaveAuto { get; set; }
        public AnalyzeCommand Analyze { get; }
        public SheetNumSelectCommand SheetNumSelect { get; }
        public RevNumSelectCommand RevNumSelect { get;  }
        public FillAlbumListCommand FillAlbumList { get; }
        public CloseAppCommand CloseApp { get; }
        public SelectSavingFolderCommand SelectSavingFolder { get; }
        
        public CreateAlbumPdfCommand CreateAlbumPdf { get; }
        public FilesToFoldersCommand FilesToFolders { get; }
        
        public BindingList<ResultField> FieldResults
        {
            get => _fieldResults;
            set
            {
                _fieldResults = value; 
                OnPropertyChanged(nameof(FieldResults));
            }
        }

        public List<ImportedFile> NewFiles
        {
            get => _newFiles;
            set
            {
                _newFiles = value;
                OnPropertyChanged(nameof(NewFiles));
            }
        }


        public List<ImportedFile> OldFiles
        {
            get => _oldFiles;
            set
            {
                _oldFiles = value;
                OnPropertyChanged(nameof(OldFiles));
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

        public string PathToSave
        {
            get => _pathToSave;
            set
            {
                _pathToSave = value; 
                OnPropertyChanged(nameof(PathToSave));
                PathToSaveAuto = false;
            }
        }

        public string PdfName
        {
            get => _pdfName;
            set
            {
                _pdfName = value; 
                OnPropertyChanged(nameof(PdfName));
            }
        }

        private bool _sheetNumSelectingEnabled;
        private bool _revNumSelectingEnabled;
        private string _delimiters;
        private BindingList<ResultField> _fieldResults;
        private List<ImportedFile> _newFiles;
        private List<ImportedFile> _oldFiles;
        private ObservableCollection<ImportedFile> _albumDrawings;
        private string _pdfName;
        private string _pathToSave;

        #endregion

        #region Methods

        public void FillFileTable(string[] files, string tableName)
        {
            var tableChooser = new Dictionary<string, List<ImportedFile>>()
            {
                {"NewDrawingsGrid", NewFiles },
                {"OldDrawingsGrid", OldFiles }
            };

            var chosenTable = tableChooser[tableName];

            TableItemsFromDirectory(files, ref chosenTable);
        }
        

        private ImportedFile TableItemFromFile(string file, bool isNewDrawing)
        {
            var itemFromFile = new ImportedFile
            {
                FullPath = Path.Combine(file),
                Name = Path.GetFileNameWithoutExtension(file),
                Extension = Path.GetExtension(file).ToLower(),
                FullName = Path.GetFileName(file),
                IsNewDrawing = isNewDrawing
            };

            return itemFromFile;
        }

        private void TableItemsFromDirectory(string[] files, ref List<ImportedFile> drawingsList)
        {
            foreach (var item in files)
            {
                if (File.Exists(item))
                {
                    bool isNewDrawing = (drawingsList == NewFiles);
                    drawingsList.Add(TableItemFromFile(item, isNewDrawing));
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
                    sheetPosition = (fieldResult.FieldSide == Side.Left) ? fieldResult.Num : fieldResult.NumFromRight;
                    sheetSide = fieldResult.FieldSide;
                }
                else if(fieldResult.IsRevNum)
                {
                    revPosition = (fieldResult.FieldSide == Side.Left) ? fieldResult.Num : fieldResult.NumFromRight;
                    revSide = fieldResult.FieldSide;
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
            SetDrawingsSheetsAndRevs(NewFiles);
            SetDrawingsSheetsAndRevs(OldFiles);
            
            AlbumDrawings.Clear();
            

            //Adding every New Drawing
            foreach (var drawing in NewFiles.Where(d => DrawingExtensions.Any(de=>de.Equals(d.Extension.ToLower()))))
            {
                AlbumDrawings.Add(drawing);
            }

            foreach (var oldDrawing in OldFiles.Where(d => DrawingExtensions.Any(de=>de.Equals(d.Extension.ToLower()))))
            {
                // Finding all OldDrawings with revision bigger (newer) than existing in table
                if (AlbumDrawings.Any(albumDrawing => albumDrawing.SheetNum == oldDrawing.SheetNum &&
                                                      albumDrawing.Extension == oldDrawing.Extension &&
                                                      albumDrawing.RevNum < oldDrawing.RevNum))
                {
                    // tempList is needed because BindingList cannot use Find()
                    var tempList = AlbumDrawings.ToList();
                    
                    var removedDrawing = tempList.Find(albumDrawing =>
                        albumDrawing.SheetNum == oldDrawing.SheetNum &&
                        albumDrawing.Extension == oldDrawing.Extension &&
                        albumDrawing.RevNum < oldDrawing.RevNum);

                    if (
                            removedDrawing.SheetNum == 0 &&
                            (
                                removedDrawing.Name.ToLower().Contains("cover") && oldDrawing.Name.ToLower().Contains("title") 
                                ||
                                removedDrawing.Name.ToLower().Contains("title") && oldDrawing.Name.ToLower().Contains("cover")
                            )
                        )
                        continue;
                    
                    AlbumDrawings.Remove(removedDrawing);
                    removedDrawing.WillNotGoToFolders = true;
                    AlbumDrawings.Add(oldDrawing);
                    continue;
                }
                
                // Finding all OldDrawings not being presented in Album before:
                if (AlbumDrawings.All(albumDrawing => 
                    (albumDrawing.SheetNum != oldDrawing.SheetNum) || 
                    (albumDrawing.SheetNum == oldDrawing.SheetNum &&
                     albumDrawing.Extension != oldDrawing.Extension)))
                {
                    AlbumDrawings.Add(oldDrawing);
                }
            }

            var dwgsToRemove = new List<ImportedFile>(AlbumDrawings.Where(ad => ad.Extension.Equals(".dwg")));
            foreach (var file in dwgsToRemove)
            {
                // Dwgs will go to folders, but not to Album PDF
                AlbumDrawings.Remove(file);
            }
            
            

            // Ordering drawings. New List is needed because BindingList cannot be OrderedBy
            var orderedDrawings = AlbumDrawings.OrderBy(x => x.SheetNum).ToList();
            AlbumDrawings.Clear();
            for (var index = 0; index < orderedDrawings.Count; index++)
            {
                var drawing = orderedDrawings[index];
                if(index>0)
                    if (drawing.SheetNum - orderedDrawings[index-1].SheetNum>1)
                    {
                        // Finding if some drawings were missed
                        // E.g.: sheets 1,2,3,5 - missing 4. 3 and 5 will be shown with red color
                        drawing.HasNumerationProblem = true;
                        orderedDrawings[index - 1].HasNumerationProblem = true;
                    }
                AlbumDrawings.Add(drawing);
            }

            if (AlbumDrawings.Count > 0)
            {
                PathToSave = Path.GetDirectoryName(AlbumDrawings[0].FullPath);
                PathToSaveAuto = true;
                try
                {
                    PdfName = GetPdfName(AlbumDrawings[0]);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private string GetPdfName(ImportedFile importedFile)
        {
            var thisFileFields = importedFile.Name.Split(string.Concat(Delimiters," ").ToCharArray());

            var thisFileResultFields = FieldResults.Where(f=> !f.IsRevNum && !f.IsSheetNum && f.FieldSide==Side.Left && f.IsConst).Select(fieldResult =>
                thisFileFields[fieldResult.Num]).ToList();

            var resultWithoutRevision = string.Join("-", thisFileResultFields.ToArray());
            var resultWithRevision =
                string.Concat(string.Concat(resultWithoutRevision, "-rev."), importedFile.RevNum);
            
            return RevisionsExist ? resultWithRevision : resultWithoutRevision;
        }

        private void FillFieldResults()
        {
            FieldResults.Clear();
            var results = new List<StringLogic.FieldColumn>(StringLogic.AnalyzeData(
                NewFiles.Select(file => file.FullName).ToList().
                    Concat(OldFiles.Select(file => file.FullName).ToList()).
                    ToList())
            );
            for (var index = 0; index < results.Count; index++)
            {
                var column = results[index];
                var thisFieldResult = new ResultField();

                if (index > 0 && column.ColumnSide == Side.Right && results[index - 1].ColumnSide == Side.Left)
                {
                    FieldResults.Add(new ResultField()
                    {
                        Value = "<....>"
                    });
                }
                    
                if (column.IsConst)
                {
                    thisFieldResult.Value = column.ConstValue;
                    thisFieldResult.IsConst = true;
                }

                thisFieldResult.IsSheetNum = column.IsSheetNum;
                thisFieldResult.IsRevNum = column.IsRevNum;
                thisFieldResult.AllValues = column.Fields.ToArray();
                thisFieldResult.FieldSide = column.ColumnSide;
                thisFieldResult.Num = column.Num;
                thisFieldResult.NumFromRight = column.NumFromRight;

                FieldResults.Add(thisFieldResult);
            }

            RevisionsExist = FieldResults.Any(f => f.IsRevNum);
        }

        private void CreateFolderStructure()
        {
            var dirPath = PathToSave;
            
            var newExtensions = NewFiles.ToList().Select(file => 
                file.Extension.Replace(".", "").ToUpper()).ToList();
            var oldExtensions = OldFiles.ToList().Select(file => 
                file.Extension.Replace(".", "").ToUpper()).ToList();

            foreach (var extension in newExtensions.Concat(oldExtensions))
            {
                Directory.CreateDirectory(Path.Combine(dirPath, extension));
                if (oldExtensions.Contains(extension))
                {
                    Directory.CreateDirectory(Path.Combine(Path.Combine(dirPath, extension), "Unchanged"));
                }
            }

            
            foreach (var file in NewFiles.Where(f => !f.WillNotGoToFolders || !DrawingExtensions.Any(de => f.Extension.Equals(de))))
            {
                var destinationFolder = Path.Combine(dirPath, file.Extension.Replace(".","").ToUpper());
                var fileName = string.Concat(file.Name, file.Extension);
                if(!File.Exists(Path.Combine(destinationFolder, fileName)))
                    File.Copy(file.FullPath, Path.Combine(destinationFolder, fileName));
            }
            
            foreach (var file in OldFiles.Where(f => !f.WillNotGoToFolders || !DrawingExtensions.Any(de => f.Extension.Equals(de))))
            {
                var destinationFolder = Path.Combine(Path.Combine(dirPath, file.Extension.Replace(".","").ToUpper()), "Unchanged");
                var fileName = string.Concat(file.Name, file.Extension);
                if(!File.Exists(Path.Combine(destinationFolder, fileName)))
                    File.Copy(file.FullPath, Path.Combine(destinationFolder, fileName));
            }

        }

        #endregion Methods

        #region Classes

        public class ResultField
        {
            public string Value { get; set; }
            public bool IsSheetNum { get; set; }
            public bool IsRevNum { get; set; }
            public bool IsConst { get; set; }
            public string[] AllValues { get; set; }
            public Side FieldSide { get; set; }
            public int Num { get; set; }
            public int NumFromRight { get; set; }

            public ResultField()
            {
                Value = "xx";
                IsRevNum = false;
                IsSheetNum = false;
                IsConst = false;
                AllValues = new string[]{};
                FieldSide = Side.Left;
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

            public bool IsNewDrawing { get; set; }
            public bool HasNumerationProblem { get; set; }
            
            public bool WillNotGoToFolders { get; set; }
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
            RevisionsExist = false;
            PathToSaveAuto = true;
            
            Delimiters = ".-_[]";
            DrawingExtensions = new List<string>
            {
                ".pdf", ".dwg", ".doc", ".docx", ".rft"
            };
            PathToSave = "";
            PdfName = "";

            NewFiles = new List<ImportedFile>();
            OldFiles = new List<ImportedFile>();
            FieldResults = new BindingList<ResultField>();
            AlbumDrawings = new ObservableCollection<ImportedFile>();
            
            Analyze = new AnalyzeCommand(this);
            SheetNumSelect = new SheetNumSelectCommand(this);
            RevNumSelect = new RevNumSelectCommand(this);
            FillAlbumList = new FillAlbumListCommand(this);
            CloseApp = new CloseAppCommand(this);
            SelectSavingFolder = new SelectSavingFolderCommand(this);
            CreateAlbumPdf = new CreateAlbumPdfCommand(this);
            FilesToFolders = new FilesToFoldersCommand(this);
            
        }

        #endregion

        
    }

}
