using System.Collections.Generic;

namespace Isop.Domain
{
    public class RecognizesConfiguration
    {
        public RecognizesConfiguration()
        {
            Recognizes = new List<Controller>();
            Properties = new List<Property>();
        }
        public IList<Controller> Recognizes { get; private set; }
        public IList<Property> Properties { get; private set; }
    }
}

