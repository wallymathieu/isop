using System.Collections.Generic;

namespace Isop.Gui
{
    public class Controller
    {
        public string Name { get; set; }

        public IEnumerable<Method> Methods { get; set; }
    }
}