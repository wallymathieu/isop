using System;
using System.Collections.Generic;
using System.Linq;
#if ! NETSTANDARD1_6
using System.Runtime.Serialization;
#endif
namespace Isop
{
    ///
#if !NETSTANDARD1_6
    [Serializable]
#endif
    public class MissingArgumentException : Exception
    {
        /// <summary>
        /// The arguments that are missing
        /// </summary>
        public IEnumerable<string> Arguments { get => (IEnumerable<string>)Data["Arguments"];
            set => Data["Arguments"] = value;
        }
        ///
        public MissingArgumentException() { }
        ///
        public MissingArgumentException(string message) : base(message) { }
        ///
        public MissingArgumentException(string message, Exception inner) : base(message, inner) { }
        ///
#if !NETSTANDARD1_6
        protected MissingArgumentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}

