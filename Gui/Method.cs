using System.Collections.Generic;

namespace Isop.Gui
{
    public class Method
    {
        public Method(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public IEnumerable<Param> Parameters { get; set; }
    }
}