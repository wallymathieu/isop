namespace Isop.CommandLine
{
    using Parse;
    public record Argument(ArgumentParameter Parameter, bool Required = false, string? Description = null)
    {
        public string Name => Parameter.LongAlias();
 
        public string Help() => Parameter.Help();

        public bool Accept(string value) => Parameter.Accept(value);

        public bool Accept(int index, string value) => Parameter.Accept(index, value);
    }
}

