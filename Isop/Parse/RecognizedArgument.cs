using System;

namespace Isop
{
    public class RecognizedArgument
    {
        public int Index { get; private set; }

        /// <summary>
        /// the matched value if any, for instance the "value" of the expression "--argument value"
        /// </summary>
        public string Value { get; private set; }
        public ArgumentWithOptions WithOptions { get; private set; }
        /// <summary>
        /// the "argument" of the expression "--argument"
        /// </summary>
        public string Argument { get; private set; }

        public bool InferredOrdinal { get; set; }

        public RecognizedArgument(ArgumentWithOptions argumentWithOptions, int index, string parameter, string value = null)
        {
            Index = index;
            Value = value;
            WithOptions = argumentWithOptions;
            Argument = parameter;
        }
        public override int GetHashCode()
        {
            return Argument.GetHashCode() + WithOptions.GetHashCode() + (Value ?? "").GetHashCode() + 1794;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;
            if (obj is RecognizedArgument)
            {
                var rec = obj as RecognizedArgument;
                return Argument.Equals(rec.Argument)
                    && WithOptions.Equals(rec.WithOptions)
                    && string.Equals(Value, rec.Value);
            }
            return false;
        }
    }
}

