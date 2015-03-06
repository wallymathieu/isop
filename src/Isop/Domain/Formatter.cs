using System;
using System.Collections.Generic;

namespace Isop.Domain
{
    public interface Formatter
    {
        IEnumerable<string> FormatCommandLine(object retval);
    }
}

