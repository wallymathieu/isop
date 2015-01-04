using System.Collections.Generic;

namespace Isop.Server.Models
{
    public class Controller
    {
        public Controller()
        {
        }

        public Controller(string name, IEnumerable<Method> actions)
        {
            Name = name;
            Methods = actions;
            Url = "/"+name;
        }
        public string Name { get; set; }

        public IEnumerable<Method> Methods { get; set; }

        public string Url { get; set; }
    }
}