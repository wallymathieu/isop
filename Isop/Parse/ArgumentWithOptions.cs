using System;

namespace Isop.Parse
{

    /// <summary>
    /// class to enable extensions of the behavior of what is recognized as arguments.
    /// </summary>
    public class ArgumentWithOptions
    {
        public string Description { get; private set; }
        public ArgumentParameter Argument { get; private set; }
        public Action<string> Action { get; set; }
        public bool Required { get; set; }
        public Type Type { get; set; }
        public ArgumentWithOptions(ArgumentParameter argument, Action<string> action = null, bool required = false, string description = null,Type type=null)
        {
            Description = description;
            Argument = argument;
            Action = action;
            Required = required;
            Type = type;
        }

        public string Help()
        {
            return Argument.Help()
                + (String.IsNullOrEmpty(Description)
                    ? ""
                    : "\t" + Description);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ArgumentWithOptions)) return false;
            return Equals((ArgumentWithOptions) obj);
        }

        public bool Equals(ArgumentWithOptions other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Description, Description) && Equals(other.Argument, Argument) && other.Required.Equals(Required);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Description != null ? Description.GetHashCode() : 0);
                result = (result*397) ^ (Argument != null ? Argument.GetHashCode() : 0);
                result = (result*397) ^ Required.GetHashCode();
                return result;
            }
        }

    }

}

