using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Isop.Gui
{
    // http://stackoverflow.com/questions/2814866/programmatically-add-controls-to-wpf-form
    /// <summary>
    /// Interaction logic for MethodView.xaml
    /// </summary>
    public partial class MethodView
    {
        public ObservableCollection<Param> Parameters { set { FieldsListBox.ItemsSource = value; } }
        public MethodView()
        {
            InitializeComponent();
        }
    }
}
