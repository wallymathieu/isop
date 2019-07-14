using System.Collections.Generic;

namespace Isop.Abstractions
{
    /// <summary>
    /// Format value as string 
    /// </summary>
    public delegate IEnumerable<string> Formatter(object value);
}

