using System;

namespace Isop.Domain
{
    public class Argument
    {
        public string Description { get; private set; }
        public string Name { get; private set; }
        public Action<string> Action { get; private set; }
        public bool Required { get; private set; }
        public Type Type { get; private set; }
        public Argument(string name, Action<string> action = null, bool required = false, string description = null,Type type=null)
        {
            Description = description;
            Name = name;
            Action = action;
            Required = required;
            Type = type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Argument)) return false;
            return Equals((Argument) obj);
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
    }
}

