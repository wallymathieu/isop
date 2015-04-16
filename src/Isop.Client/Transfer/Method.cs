using System.Collections.Generic;

namespace Isop.Client.Transfer
{
    public class Method
    {
        public Method()
        {
            Parameters = new Param[0];
        }
        public string Name { get; set; }
        public string ClassName { get; set; }
        public IEnumerable<Param> Parameters { get; set; }
        public string Help { get; set; }
        public string Url { get; set; }
    }
}