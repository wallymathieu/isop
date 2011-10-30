using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Isop.WpfControls.ViewModels
{
    public class MethodTreeModel
    {
        public ObservableCollection<Param> GlobalParameters { get; set; }
        public IEnumerable<Controller> Controllers { get; set; }
    }
}