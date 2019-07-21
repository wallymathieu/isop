using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Isop.Abstractions
{
    using CommandLine;
    using CommandLine.Parse;
    /// <summary>
    /// Parsed expression with what's recognized and what's not
    /// </summary>
    public interface IParsedExpression
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
        Task InvokeAsync(TextWriter output);
    }
}