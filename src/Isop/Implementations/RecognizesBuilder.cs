using System.Collections.Generic;

namespace Isop.Implementations
{
    using Domain;
    internal class RecognizesBuilder
    {
        public RecognizesBuilder()
        {
            Recognizes = new List<Domain.Controller>();
            Properties = new List<Property>();
        }
        public IList<Domain.Controller> Recognizes { get; }
        public IList<Property> Properties { get; }
    }
}