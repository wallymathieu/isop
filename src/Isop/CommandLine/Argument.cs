namespace Isop.CommandLine
{
    using Parse;
    public sealed class Argument
    {
        public ArgumentParameter Parameter { get; }

        public string Description { get; }
        public string Name => Parameter.LongAlias();
        public bool Required { get; }
        public Argument(ArgumentParameter parameter, bool required = false, string description = null)
        {
            Description = description;
            Parameter = parameter;
            Required = required;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Argument argument && Equals(argument);
        }

        public bool Equals(Argument other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Description, Description) && Equals(other.Name, Name) && other.Required.Equals(Required);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Description != null ? Description.GetHashCode() : 0);
                result = (result*397) ^ (Name != null ? Name.GetHashCode() : 0);
                result = (result*397) ^ Required.GetHashCode();
                return result;
            }
        }

        public string Help() => Parameter.Help();

        public bool Accept(string value) => Parameter.Accept(value);

        public bool Accept(int index, string value) => Parameter.Accept(index, value);
    }
}

