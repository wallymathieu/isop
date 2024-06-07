namespace Isop.Domain
{
    using Abstractions;
    public record Property(string Name, ArgumentAction? Action, bool Required, string? Description)
    {
        public string? Description { get; } = Description;
        public string Name { get; } = Name;
        public ArgumentAction? Action { get; } = Action;
        public bool Required { get; } = Required;
    }
}
