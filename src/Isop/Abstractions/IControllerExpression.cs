namespace Isop.Abstractions
{
    /// <summary>
    /// 
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