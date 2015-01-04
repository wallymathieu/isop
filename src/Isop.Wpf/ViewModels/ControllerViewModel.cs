using System.Collections.Generic;

namespace Isop.Gui.ViewModels
{
    public class ControllerViewModel
    {
        public string Name { get; set; }

        public IEnumerable<MethodViewModel> Methods { get; set; }
    }
}