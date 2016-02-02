using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Isop
{
    [Serializable]
    public class MissingArgumentException : Exception
    {
        /// <summary>
        /// The arguments. The key are the argument, the value is the description or help.
        /// </summary>
        public IEnumerable<string> Arguments{get{ return (IEnumerable<string>)Data["Arguments"];}set{ Data["Arguments"] = value; }}
        public MissingArgumentException() { }

        public MissingArgumentException(string message) : base(message) { }

        public MissingArgumentException(string message, Exception inner) : base(message, inner) { }
        protected MissingArgumentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

