using System.Collections.Generic;

namespace Isop.Domain
{
    /// <summary>
    /// What controllers and global arguments are recognized by Isop
    /// </summary>
    public class Recognizes
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="controllers"></param>
        /// <param name="properties"></param>
        public Recognizes(IReadOnlyList<Controller> controllers,IReadOnlyList<Property> properties)
        {
            Controllers = controllers;
            Properties = properties;
        }
        /// <summary>
        /// Controllers
        /// </summary>
        public IReadOnlyList<Controller> Controllers { get; }
        /// <summary>
        /// Global arguments
        /// </summary>
        public IReadOnlyList<Property> Properties { get; }
    }
}

