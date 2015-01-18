using System;
using System.Globalization;
using System.Collections.Generic;

namespace Isop.Domain
{
    public class Property
    {
        public string Description { get; private set; }
        public string Name { get; private set; }
        public Action<string> Action { get; set; }
        public bool Required { get; set; }
        public Type Type { get; set; }
        public Property(string name, Action<string> action = null, bool required = false, string description = null,Type type=null)
        {
            Name = name;
            Action = action;
            Required = required;
            Description = description;
            Type = type;
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
