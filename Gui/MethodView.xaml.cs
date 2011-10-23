using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Isop.Gui
{
    /// <summary>
    /// Interaction logic for MethodView.xaml
    /// </summary>
    public partial class MethodView : UserControl
    {
        public ObservableCollection<Param> Parameters { set { FieldsListBox.ItemsSource = value; } }
        public MethodView()
        {
            InitializeComponent();
        }
    }
}
