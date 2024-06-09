namespace Isop.Abstractions;
/// <summary>
/// This interface represents a controller given by <see cref="Name"/>.
/// </summary>
public interface IController
{
    /// <summary>
    /// Get help for controller
    /// </summary>
    string Help();
    /// <summary> ̰
    /// Get controller action with <paramref name="name"/>.
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
