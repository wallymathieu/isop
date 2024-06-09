using System.Collections.Generic;

namespace Isop.Domain;
/// <summary>
/// What controllers and global arguments are recognized by Isop
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="controllers"></param>
/// <param name="properties"></param>
public class Recognizes(IReadOnlyList<Controller> controllers, IReadOnlyList<ArgumentWithAction> properties)
{
    /// <summary>
    /// Controllers
    /// </summary>
    public IReadOnlyList<Controller> Controllers { get; } = controllers;
    /// <summary>
    /// Global arguments
    /// </summary>
    public IReadOnlyList<ArgumentWithAction> Properties { get; } = properties;
}


