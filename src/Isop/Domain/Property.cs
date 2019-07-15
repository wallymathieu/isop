namespace Isop.Domain
{
    using Abstractions;
    public class Property
    {
        public string Description { get; }
        public string Name { get; }
        public ArgumentAction Action { get; }
        public bool Required { get; }
        public Property(string name, ArgumentAction action = null, bool required = false, string description = null)
        {
            Name = name;
            Action = action;
            Required = required;
            Description = description;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Property)) return false;
            return Equals((Property) obj);
        }

        public bool Equals(Property other)
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

    }
}
