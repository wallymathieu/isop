namespace Isop.CommandLine.Parse
{
    public class UnrecognizedArgument
    {
        public UnrecognizedArgument(int index, string value)
        {
            Index = index;
            Value = value;
        }

        public int Index { get; }
        public string Value { get; }
        public override string ToString()
        {
            return Value;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(UnrecognizedArgument)) return false;
            return Equals((UnrecognizedArgument)obj);
        }

        public bool Equals(UnrecognizedArgument other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Index == Index && Equals(other.Value, Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Index * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }
}

