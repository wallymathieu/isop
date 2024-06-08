using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Isop.Abstractions
{
    /// <summary>
    /// AppHost contains the service provider and configuration needed to run a command line app
    /// </summary>
    public interface IAppHost
    {
        /// <summary>
        /// Parse command line arguments and return parsed arguments entity
        /// </summary>
        IParsed Parse(IEnumerable<string> arg);

        /// <summary>
        /// Return help-text
        /// </summary>
        [Obsolete("Prefer HelpAsync")] string Help();

        /// <summary>
        /// Return help-text
        /// </summary>
        Task<string> HelpAsync();

        /// <summary>
        /// Get controller with name
        /// </summary>
        /// <exception cref="ArgumentException">Throws an argument exception if controller is not found.</exception>
        IController Controller(string controllerName);

        /// <summary>
        /// Controllers exposed by the API
        /// </summary>
        IReadOnlyList<IController> Controllers { get; }
    }
}