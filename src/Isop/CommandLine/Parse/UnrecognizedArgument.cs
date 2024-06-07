namespace Isop.CommandLine.Parse
{
    public record UnrecognizedArgument(int Index, string Value)
    {
        public override string ToString()
        {
            return Value;
        }
    }
}

