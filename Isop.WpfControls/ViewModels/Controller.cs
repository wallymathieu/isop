using System.Collections.Generic;

namespace Isop.WpfControls.ViewModels
{
    public class Controller
    {
        public string Name { get; set; }

        public IEnumerable<Method> Methods { get; set; }
    }
}