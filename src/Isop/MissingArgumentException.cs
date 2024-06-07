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
        public IReadOnlyCollection<string> Arguments
        {
            get => (IReadOnlyCollection<string>)Data["Arguments"]!;
            set => Data["Arguments"] = value;
        }
        ///
        public MissingArgumentException(string message, IReadOnlyCollection<string> arguments) : base(message) { Arguments = arguments; }

        ///
#if !NETSTANDARD1_6
        protected MissingArgumentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
    }
}

