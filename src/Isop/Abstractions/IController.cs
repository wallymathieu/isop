using System.Collections.Generic;

namespace Isop.Abstractions
{
    /// <summary>
    /// Controller expression in order to get help for controller or specify action
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// Get help for controller
        /// </summary>
        string Help();
        /// <summary>
        /// Get action expression
        /// </summary>
        /// <param name="name">action name</param>
        /// <returns></returns>
        IActionOnController Action(string name);
        /// <summary>
        /// Actions exposed by controller
        /// </summary>
        IReadOnlyCollection<IActionOnController> Actions { get; }
        /// <summary>
        /// Name of controller
        /// </summary>
        string Name { get; }
    }
}