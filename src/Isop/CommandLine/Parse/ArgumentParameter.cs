using System;
using System.Linq;
using System.Collections.Generic;

namespace Isop.CommandLine.Parse
{
    using Infrastructure;
    using Parameters;

    /// <summary>
    /// Represents the parameter. For instance "file" of the commandline argument --file. 
    /// </summary>
    public class ArgumentParameter
    {
        public ArgumentParameter(string prototype, IEnumerable<string> names, string delimiter = null, int? ordinal = null)
        {
            Prototype = prototype;
            Aliases = names.ToArray();
            Delimiter = delimiter;
            Ordinal = ordinal;
        }

        public string Prototype { get; }
        public int? Ordinal { get; }

        public static ArgumentParameter Parse(string value, IFormatProvider formatProvider)
        {
            ArgumentParameter ordinalParameter;
            if (OrdinalParameter.TryParse(value, formatProvider, out ordinalParameter))
                return ordinalParameter;
            ArgumentParameter optionParameter;
            if (OptionParameter.TryParse(value, out optionParameter))
                return optionParameter;
            ArgumentParameter visualStudioParameter;
            if (VisualStudioParameter.TryParse(value, out visualStudioParameter))
                return visualStudioParameter;
            throw new ArgumentOutOfRangeException(value);
        }

        public ICollection<string> Aliases { get; }
        public string Delimiter { get; }
        public string Help()
        {
            return string.Concat( 
                "--", 
                string.Join(", or ", Aliases), 
                (string.IsNullOrEmpty(Delimiter)
                    ? ""
                    : " " + Delimiter));
        }
        public override string ToString()
        {
            return Prototype;
        }

        public bool HasAlias(string value)
        {
            return Aliases.Any(value.EqualsIgnoreCase);
        }

        public bool Accept(int index, string val)
        {
            if (!Ordinal.HasValue)
                return HasAlias(val);
            else
                return Ordinal.Value.Equals(index) && HasAlias(val);

        }

        public string LongAlias()
        {
            var maxLength = Aliases.Max(a => a.Length);
            return Aliases.SingleOrDefault(a => a.Length == maxLength);
        }

        public bool Accept(string value)
        {
            return HasAlias(value);
        }
    }
}

