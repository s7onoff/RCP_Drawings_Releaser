using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace RCP_Drawings_Releaser.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SheetNumSelectingEnabled = false;
            RevNumSelectingEnabled = false;
        }
        
        private void drawings_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                
                var vm = (ViewModels.MainWindowVM)this.MainGrid.DataContext;
                
                vm.FillFileTable(files, ((DataGrid)sender).Name);
                
                ((DataGrid)sender).Items.Refresh();
            }
        }

        public bool SheetNumSelectingEnabled { get; set; }
        public bool RevNumSelectingEnabled { get; set; }

        
        
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
        private void SheetNum_OnClick(object sender, RoutedEventArgs e)
        {
            RevNumSelectingEnabled = false;
            SheetNumSelectingEnabled = true;
        }

        private void RevNum_OnClick(object sender, RoutedEventArgs e)
        {
            SheetNumSelectingEnabled = false;
            RevNumSelectingEnabled = true;
        }

        private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            if (SheetNumSelectingEnabled)
            {
                var vm = (ViewModels.MainWindowVM)MainGrid.DataContext;
                var resultField = (ViewModels.MainWindowVM.ResultField)((ListBoxItem)sender).Content;

                vm.ChangeListNumField(resultField);
                //todo:can't understand how to update
                MainGrid.UpdateLayout();
            }
            
            SheetNumSelectingEnabled = false;
            RevNumSelectingEnabled = false;
        }
    }
}
