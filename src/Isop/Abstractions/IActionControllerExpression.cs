using System.Collections.Generic;

namespace Isop.Abstractions
{
    using CommandLine;
    /// <summary>
    /// 
    /// </summary>
    public interface IActionControllerExpression
    {
        /// <summary>
        /// Get arguments for controller action
        /// </summary>
        IReadOnlyCollection<Argument> GetArguments();

        /// <summary>
        /// send parameters to controller actions
        /// </summary>
        IParsedExpression Parameters(Dictionary<string, string> parameters);

        /// <summary>
        /// Get help for controller action
        /// </summary>
        string Help();
    }
}