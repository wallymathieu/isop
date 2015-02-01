using System.Collections.Generic;

namespace Isop.Server.Models
{
    public class Controller
    {
        public Controller()
        {
            Methods = new Method[0];
        }

        public Controller(string name, IEnumerable<Method> methods)
            :this()
        {
            Name = name;
            Methods = methods;
        }
        public string Name { get; set; }

        public IEnumerable<Method> Methods { get; set; }
    }
}