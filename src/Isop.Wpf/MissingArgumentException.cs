using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isop.Gui
{
    [Serializable]
    public class MissingArgumentException : Exception
    {
        /// <summary>
        /// The arguments. The key are the argument, the value is the description or help.
        /// </summary>
        public IDictionary<string, string> Arguments;
        public MissingArgumentException() { }

        public MissingArgumentException(string message) : base(message) { }

        public MissingArgumentException(string message, Exception inner) : base(message, inner) { }
    }
}
