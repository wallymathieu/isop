using System.Collections.ObjectModel;
using System.Windows;

namespace Isop.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TreeOfControllers TreeOfControllers { get; set; }
        public ObservableCollection<Param> Parameters;
        public MainWindow()
        {
            TreeOfControllers= new TreeOfControllers();

            InitializeComponent();
            controllersAndCommands.DataContext = TreeOfControllers.Controllers;
            textBlock1.Text = string.Empty;
        }

        private void SelectedMethodChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Method) 
            {
                methodview.DataContext = e.NewValue;
                Parameters = new ObservableCollection<Param>();
                foreach (var parameter in (e.NewValue as Method).Parameters)
                {
                    Parameters.Add(parameter);
                }
                methodview.Parameters = Parameters;
                textBlock1.Text = string.Empty;
            }
        }
    }
}
