using System.Collections.Generic;

namespace Isop.Gui.Models
{
    public class Method
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public IEnumerable<Param> Parameters { get; set; }
        public string Help { get; private set; }
        public string Url { get; set; }
    }
}