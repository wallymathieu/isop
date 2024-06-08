using Isop.Abstractions;
using Isop.CommandLine.Parse;
using Isop.CommandLine;

namespace Isop.Domain
{
    /// <summary>A property is an argument with an optional action. Used for global arguments.</summary>
    public class Property(
        string name,
        ArgumentAction? action,
        bool required,
        string? description): 
        Argument(
            required: required,
            description: description,
            parameter: ArgumentParameter.Parse(name, null))
    {
        public ArgumentAction? Action { get; } = action;
    }
}
