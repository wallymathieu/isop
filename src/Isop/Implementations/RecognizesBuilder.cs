using System.Collections.Generic;
using Isop.Domain;

namespace Isop.Implementations
{
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