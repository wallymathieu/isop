using System.Collections.Generic;

namespace Isop.Implementations
{
    using Domain;
    internal sealed class RecognizesBuilder
    {
        public RecognizesBuilder()
        {
            Recognizes = [];
            Properties = [];
        }
        public IList<Domain.Controller> Recognizes { get; }
        public IList<ArgumentWithAction> Properties { get; }
    }
}