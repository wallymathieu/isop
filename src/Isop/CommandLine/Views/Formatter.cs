using System.Collections.Generic;

namespace Isop.CommandLine.Views
{
    /// <summary>
    /// Format value as string 
    /// </summary>
    public delegate IEnumerable<string> Formatter(object value);
}

