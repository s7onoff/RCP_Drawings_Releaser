using System.Windows;
using System.Windows.Controls;

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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
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
    }
}
