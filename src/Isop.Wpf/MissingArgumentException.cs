using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isop.Gui
{
    #if !PCL
    [Serializable]
    #endif
    public class MissingArgumentException : Exception
    {
        /// <summary>
        /// The arguments. The key are the argument, the value is the description or help.
        /// </summary>
        public string[] Arguments;
        public MissingArgumentException() { }

        public MissingArgumentException(string message) : base(message) { }

        public MissingArgumentException(string message, Exception inner) : base(message, inner) { }
    }
}
