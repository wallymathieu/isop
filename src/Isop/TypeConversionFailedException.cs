using System;
#if ! NETSTANDARD1_6
using System.Runtime.Serialization;
#endif

namespace Isop
{
    /// <summary>
    /// Could not convert to type
    /// </summary>
#if !NETSTANDARD1_6
    [Serializable]
#endif
    public class TypeConversionFailedException : Exception
    {
        /// <summary>
        /// The argument name
        /// </summary>
        public string Argument { get => (string)Data["Argument"];
            set => Data["Argument"] = value;
        }
        /// <summary>
        /// The argument value
        /// </summary>
        public string Value { get => (string)Data["Value"];
            set => Data["Value"] = value;
        }
        /// <summary>
        /// The intended target type to convert to
        /// </summary>
        public Type TargetType { get => (Type)Data["TargetType"];
            set => Data["TargetType"] = value;
        }
        ///
        public TypeConversionFailedException()
        {
        }
        ///
        public TypeConversionFailedException(string message) : base(message)
        {
        }
        ///
        public TypeConversionFailedException(string message, Exception inner) : base(message, inner)
        {
        }
        ///
#if !NETSTANDARD1_6
        protected TypeConversionFailedException(SerializationInfo info, StreamingContext context) : base(info, context){}
#endif
    }
}

