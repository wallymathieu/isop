using System;

namespace Isop.Server.Models
{
    public class Param 
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public bool Required { get; set; }
        public Param()
        {
        }

        public Param(Type type, string name, bool required=false)
        {
            Type = type.FullName;
            Name = name;
            Required = required;
        }
    }
}