namespace Isop.Domain
{
    using Abstractions;
    public class Property(string name, ArgumentAction? action, bool required, string? description)
    {
        public string? Description { get; } = description;
        public string Name { get; } = name;
        public ArgumentAction? Action { get; } = action;
        public bool Required { get; } = required;
    }
}
