namespace Isop.Abstractions
{
    /// <summary>
    /// Controller expression in order to get help for controller or specify action
    /// </summary>
    public interface IControllerExpression
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
        IActionControllerExpression Action(string name);
    }
}