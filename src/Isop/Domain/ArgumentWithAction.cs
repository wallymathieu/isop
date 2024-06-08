using Isop.Abstractions;
using Isop.CommandLine.Parse;
using Isop.CommandLine;

namespace Isop.Domain
{
    /// <summary>An argument with an optional action. Used for global arguments.</summary>
    public class ArgumentWithAction(
        ArgumentParameter parameter,
        ArgumentAction? action,
        bool required,
        string? description): 
        Argument(
            required: required,
            description: description,
            parameter: parameter)
    {
        public ArgumentAction? Action { get; } = action;
    }
}
