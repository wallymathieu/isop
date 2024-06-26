#pragma warning disable CA1034 // Do not nest type
using System.Linq;
namespace Isop.CommandLine.Parse;

public abstract class ParsedArguments
{
    private ParsedArguments() { }

    public ParsedArguments Merge(ParsedArguments args)
    {
        return new Merged(this, args);
    }

    /// <summary>
    /// A combination of two parsed arguments instances
    /// </summary>
    public class Merged(ParsedArguments first, ParsedArguments second) : ParsedArguments
    {
        /// <summary>
        /// 
        /// </summary>
        public ParsedArguments First { get; } = first;
        /// <summary>
        /// 
        /// </summary>
        public ParsedArguments Second { get; } = second;
    }

    public class MethodMissingArguments(Type recognizedClass,
        Domain.Method recognizedAction,
        IReadOnlyCollection<string> missingParameters) : ParsedArguments
    {
        public Type RecognizedClass { get; } = recognizedClass;
        public Domain.Method RecognizedAction { get; } = recognizedAction;
        public IReadOnlyCollection<string> MissingParameters { get; } = missingParameters;
    }

    public class Method(Type recognizedClass,
        Domain.Method recognizedAction,
        IEnumerable<object> recognizedActionParameters,
        IEnumerable<RecognizedArgument> recognized) : ParsedArguments
    {
        public Type RecognizedClass { get; } = recognizedClass;
        public Domain.Method RecognizedAction { get; } = recognizedAction;
        public IEnumerable<RecognizedArgument> Recognized { get; } = recognized;
        public IReadOnlyCollection<object> RecognizedActionParameters { get; } = recognizedActionParameters.ToArray();
    }

    public class Properties(IReadOnlyCollection<RecognizedArgument> recognized,
        IReadOnlyCollection<UnrecognizedArgument> unrecognized) : ParsedArguments
    {
        /// <summary>
        /// Recognized arguments
        /// </summary>
        public IReadOnlyCollection<RecognizedArgument> Recognized { get; } = recognized;

        /// <summary>
        /// Unrecognized arguments
        /// </summary>
        public IReadOnlyCollection<UnrecognizedArgument> Unrecognized { get; } = unrecognized;
    }

    /// <summary>
    /// Map from <see cref="ParsedArguments"/> to <see typecref="T"/>.
    /// </summary>
    public T Select<T>(Func<Method, T> method, Func<Merged, T> merged, Func<Properties, T> properties, Func<MethodMissingArguments, T> methodMissingArguments)
    {
        if (method is null) throw new ArgumentNullException(nameof(method));
        if (merged is null) throw new ArgumentNullException(nameof(merged));
        if (properties is null) throw new ArgumentNullException(nameof(properties));
        if (methodMissingArguments is null) throw new ArgumentNullException(nameof(methodMissingArguments));
        return this switch
        {
            Method pm => method(pm),
            Merged m => merged(m),
            Properties d => properties(d),
            MethodMissingArguments e => methodMissingArguments(e),
            _ => throw new InvalidOperationException("Unimplemented switch case"),
        };
    }

}