using System.Collections.Generic;

namespace Isop.Client.Transfer
{
    public class Method
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public IEnumerable<Param> Parameters { get; set; }
        public string Help { get; set; }
        public string Url { get; set; }
    }
}