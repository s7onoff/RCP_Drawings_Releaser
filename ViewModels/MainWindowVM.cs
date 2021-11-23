using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        
        
        
        #endregion

        #region Props

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
            
            UpdateFileFields();
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

        private void UpdateFileFields()
        {
            var allFileNames = NewDrawings.Select(file => file.FullName).ToList();
            allFileNames.AddRange(OldDrawings.Select(file => file.FullName));
            
            StringLogic.AnalyzeData(allFileNames, Side.Left, Side.Left);
        }

        #endregion

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

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            private void OnDrawingPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Ctor

        public MainWindowVM()
        {
            NewDrawings = new List<ImportedFile>();
            OldDrawings = new List<ImportedFile>();
        }

        #endregion
    }

}
