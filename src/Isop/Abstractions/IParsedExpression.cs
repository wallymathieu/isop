using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Isop.CommandLine;
using Isop.CommandLine.Parse;

namespace Isop.Abstractions
{
    /// <summary>
    /// 
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
        /// <summary>
        /// Global arguments
        /// </summary>
        IReadOnlyCollection<Argument> GlobalArguments { get; }
        /// <summary>
        /// Invoke using parameters
        /// </summary>
        Task InvokeAsync(TextWriter output);
    }
}