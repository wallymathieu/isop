using System.Reflection;

namespace Isop.CommandLine.Parse
{
    using Domain;
    using Infrastructure;

    public class RecognizedArgument
    {
        public int Index { get; }

        /// <summary>
        /// the matched value if any, for instance the "value" of the expression "--argument value"
        /// </summary>
        public string Value { get; }
        public Argument Argument { get; }
        /// <summary>
        /// the "argument" of the expression "--argument"
        /// </summary>
        public string RawArgument { get; }

        public bool InferredOrdinal { get; set; }

        public RecognizedArgument(Argument argumentWithOptions, int index, string argument, string value = null)
        {
            Index = index;
            Value = value;
            Argument = argumentWithOptions;
            RawArgument = argument;
        }
        public override int GetHashCode()
        {
            return RawArgument.GetHashCode() + Argument.GetHashCode() + (Value ?? "").GetHashCode() + 1794;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;
            var recognizedArgument = obj as RecognizedArgument;
            if (recognizedArgument != null)
            {
                return RawArgument.Equals(recognizedArgument.RawArgument)
                    && Argument.Equals(recognizedArgument.Argument)
                    && string.Equals(Value, recognizedArgument.Value);
            }
            return false;
        }

        public bool Matches(Parameter paramInfo)
        {
            return RawArgument.EqualsIgnoreCase(paramInfo.Name);
        }

        public bool Matches(PropertyInfo prop)
        {
            return RawArgument.EqualsIgnoreCase(prop.Name);
        }
    }
}

