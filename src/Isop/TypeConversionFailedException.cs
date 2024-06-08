using System;

namespace Isop
{
    /// <summary>
    /// Could not convert to type
    /// </summary>
    public class TypeConversionFailedException : Exception
    {
        /// <summary>
        /// The argument name
        /// </summary>
        public string? Argument
        {
            get => (string?)Data["Argument"];
            set => Data["Argument"] = value;
        }
        /// <summary>
        /// The argument value
        /// </summary>
        public string? Value
        {
            get => (string?)Data["Value"];
            set => Data["Value"] = value;
        }
        /// <summary>
        /// The intended target type to convert to
        /// </summary>
        public Type? TargetType
        {
            get => (Type?)Data["TargetType"];
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
    }
}

