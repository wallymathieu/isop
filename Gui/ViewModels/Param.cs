using System;

namespace Isop.Gui
{
    public class Param
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public Param(Type type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}