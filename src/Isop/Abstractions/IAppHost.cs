using System;
using System.Collections.Generic;

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
        String Help();

        /// <summary>
        /// 
        /// </summary>
        IController Controller(string controllerName);
        /// <summary>
        /// Controllers exposed by the API
        /// </summary>
        IReadOnlyList<IController> Controllers { get; }
    }
}