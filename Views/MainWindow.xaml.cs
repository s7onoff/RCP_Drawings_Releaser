using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using PdfSharp.Pdf;
using RCP_Drawings_Releaser.ViewModels;
using WPF.JoshSmith.ServiceProviders.UI;

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
            new ListViewDragDropManager<MainWindowVM.ImportedFile>(FilesToCombine);
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
     
        
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            var vm = (ViewModels.MainWindowVM)MainGrid.DataContext;
            
            if (vm.SheetNumSelectingEnabled)
            {
                var resultField = (ViewModels.MainWindowVM.ResultField)((ListBoxItem)sender).Content;

                vm.ChangeListNumField(resultField);
                FieldsListBox.Items.Refresh();
            }
            else if (vm.RevNumSelectingEnabled)
            {
                var resultField = (ViewModels.MainWindowVM.ResultField)((ListBoxItem)sender).Content;

                vm.ChangeRevNumField(resultField);
                FieldsListBox.Items.Refresh();
            }
            
            vm.SheetNumSelectingEnabled = false;
            vm.RevNumSelectingEnabled = false;
        }
    }
}
