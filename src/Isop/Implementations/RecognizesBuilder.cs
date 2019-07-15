using System.Collections.Generic;

namespace Isop.Implementations
{
    using Domain;
    internal class RecognizesBuilder
    {
        public RecognizesBuilder()
        {
            Recognizes = new List<Controller>();
            Properties = new List<Property>();
        }
        public IList<Controller> Recognizes { get; }
        public IList<Property> Properties { get; }
    }
}