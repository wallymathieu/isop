using System.Collections.Generic;
using Isop.Domain;

namespace Isop.Implementations;
internal sealed class RecognizesBuilder
{
    public RecognizesBuilder()
    {
        Recognizes = [];
        Properties = [];
    }
    public IList<Controller> Recognizes { get; }
    public IList<ArgumentWithAction> Properties { get; }
}
