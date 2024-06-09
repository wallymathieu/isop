using System.Collections.Generic;

namespace Isop.CommandLine.Parse;
public class RecognizedArgument(Argument argument,
    IReadOnlyCollection<int> index,
    string rawArgument,
    string? value = null,
    bool inferredOrdinal = false)
{
    public IReadOnlyCollection<int> Index { get; } = index;

    /// <summary>
    /// the matched value if any, for instance the "value" of the expression "--argument value"
    /// </summary>
    public string? Value { get; } = value;
    public Argument Argument { get; } = argument;
    /// <summary>
    /// the "argument" of the expression "--argument"
    /// </summary>
    public string RawArgument { get; } = rawArgument;

    public bool InferredOrdinal { get; } = inferredOrdinal;
}


