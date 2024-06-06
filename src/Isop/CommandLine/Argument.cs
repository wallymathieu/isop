namespace Isop.CommandLine
{
    using Parse;
    public sealed class Argument(ArgumentParameter parameter, bool required = false, string description = null)
    {
        public ArgumentParameter Parameter { get; } = parameter;

        public string Description { get; } = description;
        public string Name => Parameter.LongAlias();
        public bool Required { get; } = required;

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Argument argument && Equals(argument);
        }

        public bool Equals(Argument other)
        {
            if (other is null) return false;
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

