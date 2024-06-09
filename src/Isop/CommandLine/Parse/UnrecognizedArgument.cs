namespace Isop.CommandLine.Parse;
public class UnrecognizedArgument(int index, string value)
{
    public int Index { get; } = index;
    public string Value { get; } = value;
    public override string ToString()
    {
        return Value;
    }
}


