using System.Collections.Generic;
using Isop.Domain;
namespace Isop.Api
{
    public class RecognizesConfigurationBuilder
    {
        public RecognizesConfigurationBuilder()
        {
            Recognizes = new List<Controller>();
            Properties = new List<Property>();
        }
        public IList<Controller> Recognizes { get; }
        public IList<Property> Properties { get; }
    }
}