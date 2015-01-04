using System.Collections.Generic;

namespace Isop.Server.Models
{
    public class Method
    {
        public Method()
        {
        }

        public Method(string name, string className, string help)
        {
            Name = name;
            ClassName = className;
            Help = help;
            Url = "/" + className + "/" + name;
        }

        public string Name { get; set; }
        public string ClassName { get; set; }

        public IEnumerable<Param> Parameters { get; set; }
        public string Help { get; private set; }
        public string Url { get; set; }
    }
}