using System;

namespace Isop.Client.Models
{
    public class Param 
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Required { get; set; }
    }
}