using System.Windows;
using Isop.Gui.ViewModels;
using System.Collections.Generic;

namespace Isop.Gui
{
    // http://stackoverflow.com/questions/2814866/programmatically-add-controls-to-wpf-form
    /// <summary>
    /// Interaction logic for MethodView.xaml
    /// </summary>
    public partial class MethodView
    {

        public IEnumerable<ParamViewModel> Source
        {
            set { FieldsListBox.ItemsSource = value; }
        }

        public MethodView()
        {
            InitializeComponent();
        }
    }
}
