using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Isop.Gui
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

        public ObservableCollection<Param> Parameters { get; set; }
    }
}