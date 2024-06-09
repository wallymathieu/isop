using System.Collections.Generic;
using Isop.CommandLine;

namespace Isop.Abstractions;
/// <summary>
/// An action on a controller is a single method. This interface represents
/// such a method.
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
    IParsed Parameters(IReadOnlyCollection<KeyValuePair<string, string?>> parameters);

    /// <summary>
    /// Get help for controller action
    /// </summary>
    string Help();
    /// <summary>
    /// Action name
    /// </summary>
    string Name { get; }
}
