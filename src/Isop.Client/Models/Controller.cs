using System.Collections.Generic;

namespace Isop.Client.Models
{
    public class Controller
    {
        public string Name { get; set; }
        public IEnumerable<Method> Methods { get; set; }
        public string Url { get; set; }
    }
}