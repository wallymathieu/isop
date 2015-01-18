using System;
using System.Collections.Generic;

namespace Isop
{
    public class MissingArgumentException : Exception
    {
        /// <summary>
        /// The arguments. The key are the argument, the value is the description or help.
        /// </summary>
        public string[] Arguments{get{ return (string[])Data["Arguments"];}set{ Data["Arguments"] = value; }}
        public MissingArgumentException() { }

        public MissingArgumentException(string message) : base(message) { }

        public MissingArgumentException(string message, Exception inner) : base(message, inner) { }
    }
}

