using System.Collections.Generic;

namespace Isop.Abstractions
{
    using CommandLine;
    /// <summary>
    /// 
    /// </summary>
    public interface IActionOnController
    {
        /// <summary>
        /// Get arguments for controller action
        /// </summary>
        IReadOnlyCollection<Argument> Arguments { get; }

        /// <summary>
        /// send parameters to controller actions
        /// </summary>
        IParsed Parameters(Dictionary<string, string> parameters);

        /// <summary>
        /// Get help for controller action
        /// </summary>
        string Help();
        /// <summary>
        /// Action name
        /// </summary>
        string Name { get; }
    }
}