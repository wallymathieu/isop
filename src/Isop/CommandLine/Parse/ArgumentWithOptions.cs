using System;

namespace Isop.CommandLine.Parse
{
    using Domain;
    /// <summary>
    /// class to enable extensions of the behavior of what is recognized as arguments.
    /// </summary>
    public class ArgumentWithOptions : Argument
    {
        public ArgumentParameter Argument { get; private set; }
        public ArgumentWithOptions(ArgumentParameter argument, Action<string> action = null, bool required = false, string description = null,Type type=null)
            :base(argument!=null? argument.LongAlias() : null, action, required, description, type)
        {
            Argument = argument;
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

