using System.Collections.Generic;
using Xamarin.Forms;

namespace Isop.Xamarin
{
    public class ControllerViewModel
    {
        public string Name { get; set; }

        public IEnumerable<MethodViewModel> Methods { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}