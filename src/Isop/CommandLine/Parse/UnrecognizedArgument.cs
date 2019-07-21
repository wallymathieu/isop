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
    }
}

