using System.Collections.Generic;

namespace Isop.Client.Transfer
{
    public class Controller
    {
        public string Name { get; set; }
        public IEnumerable<Method> Methods { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}