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
        public IList<Controller> Recognizes { get; private set; }
        public IList<Property> Properties { get; private set; }
    }
}