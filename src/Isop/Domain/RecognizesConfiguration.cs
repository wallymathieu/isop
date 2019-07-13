using System.Collections.Generic;

namespace Isop.Domain
{
    public class RecognizesConfiguration
    {
        public RecognizesConfiguration(IReadOnlyList<Controller> recognizes,IReadOnlyList<Property> properties)
        {
            Recognizes = recognizes;
            Properties = properties;
        }
        public IReadOnlyList<Controller> Recognizes { get; }
        public IReadOnlyList<Property> Properties { get; }
    }
}

