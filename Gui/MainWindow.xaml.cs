using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TreeOfControllers TreeOfControllers { get; set; }
        public MainWindow()
        {
            TreeOfControllers= new TreeOfControllers();

            InitializeComponent();
            treeView1.DataContext = TreeOfControllers.Controllers;
        }
    }
}
