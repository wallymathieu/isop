using System.Collections.Generic;

namespace Isop.WpfControls.ViewModels
{
    public class Method
    {
        public Method(string name, string className)
        {
            Name = name;
            ClassName = className;
        }

        public string Name { get; set; }
        public string ClassName { get; set; }

        public IEnumerable<Param> Parameters { get; set; }
    }
}