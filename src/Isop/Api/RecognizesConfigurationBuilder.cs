using System.Collections.Generic;
using Isop.Domain;
namespace Isop.Api
{
    /// <summary>
    /// 
    /// </summary>
    public class RecognizesConfigurationBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        public RecognizesConfigurationBuilder()
        {
            Recognizes = new List<Controller>();
            Properties = new List<Property>();
        }
        /// <summary>
        /// 
        /// </summary>
        public IList<Controller> Recognizes { get; }
        /// <summary>
        /// 
        /// </summary>
        public IList<Property> Properties { get; }
    }
}