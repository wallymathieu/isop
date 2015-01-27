using Isop.Gui.ViewModels;
using Microsoft.Win32;
using System;
using System.Windows;

namespace Isop.Gui
{
    /// <summary>
    /// Interaction logic for SelectClient.xaml
    /// </summary>
    public partial class SelectClient : Window
    {
        FromDroppedFile fromDroppedFile = new FromDroppedFile();
        public SelectClient()
        {
            this.DataContext = new SelectClientViewModel();
            InitializeComponent();
        }

        public SelectClientViewModel Source
        {
            get { return (SelectClientViewModel)this.DataContext; }
        }

        public void GetAssembly()
        {
            var dlg = new OpenFileDialog();

            dlg.DefaultExt = ".dll";
            dlg.Filter = "Dll (*.dll)|*.dll|Exe (*.exe)|*.exe";

            var result = dlg.ShowDialog();
            if (result == true)
            {
                Source.Assembly = dlg.FileName;
            }
        }

        private void Path_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GetAssembly();
        }

        public void Window_Drop(object sender, DragEventArgs e)
        {
            Source.Assembly = fromDroppedFile.GetFileName(e);
            Source.OnLoad();
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            Source.OnLoad();
            Hide();
        }
    }
}
