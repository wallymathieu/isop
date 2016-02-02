using System;
using System.Runtime.Serialization;

namespace Isop
{
    [Serializable]
    public class TypeConversionFailedException : Exception
    {
        public string Argument { get { return (string)Data["Argument"]; } set { Data["Argument"] = value; } }
        public string Value { get { return (string)Data["Value"]; } set { Data["Value"] = value; } }
        public Type TargetType { get { return (Type)Data["TargetType"]; } set { Data["TargetType"] = value; } }
        public TypeConversionFailedException()
        {
        }

        public TypeConversionFailedException(string message) : base(message)
        {
        }

        public TypeConversionFailedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected TypeConversionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

