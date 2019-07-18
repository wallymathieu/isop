namespace Isop.Domain
{
    using Abstractions;
    public class Property
    {
        public string Description { get; }
        public string Name { get; }
        public ArgumentAction Action { get; }
        public bool Required { get; }
        public Property(string name, ArgumentAction action, bool required, string description)
        {
            Name = name;
            Action = action;
            Required = required;
            Description = description;
        }
    }
}
