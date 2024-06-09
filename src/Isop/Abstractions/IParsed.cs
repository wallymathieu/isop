using System.IO;
using Isop.CommandLine.Parse;

namespace Isop.Abstractions;
/// <summary>
/// Parsed expression with what's recognized and what's not
/// </summary>
public interface IParsed
{
    /// <summary>
    /// Recognized arguments
    /// </summary>
    IReadOnlyCollection<RecognizedArgument> Recognized { get; }

    /// <summary>
    /// Unrecognized arguments
    /// </summary>
    IReadOnlyCollection<UnrecognizedArgument> Unrecognized { get; }
    // TODO: IReadOnlyCollection<Argument> PotentialArguments { get; }
    /// <summary>
    /// Invoke using parameters
    /// </summary>
    Task InvokeAsync(TextWriter? output);
    /// <summary>
    /// Invoke using parameters
    /// </summary>
    Task<IEnumerable<InvokeResult>> InvokeAsync();

    /// <summary>
    /// Return help text
    /// </summary>
    [Obsolete("Prefer HelpAsync")] string Help();
    /// <summary>
    /// Return help text
    /// </summary>
    public Task<string> HelpAsync();
}
