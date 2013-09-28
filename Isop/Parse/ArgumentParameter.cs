using System;
using System.Linq;
using System.Globalization;
using Isop.Infrastructure;
using Isop.Parse.Parameters;

namespace Isop.Parse
{
    /// <summary>
    /// Represents the parameter. For instance "file" of the commandline argument --file. 
    /// </summary>
    public class ArgumentParameter
    {
        public ArgumentParameter(string prototype, string[] names, string delimiter = null, int? ordinal = null)
        {
            Prototype = prototype;
            Aliases = names;
            Delimiter = delimiter;
            Ordinal = ordinal;
        }

        public string Prototype { get; protected set; }
        public int? Ordinal { get; protected set; }

        public static implicit operator ArgumentParameter(string value)
        {
            return Parse(value, CultureInfo.CurrentCulture);
        }

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

        public string[] Aliases { get; protected set; }
        public string Delimiter { get; protected set; }
        public string Help()
        {
            return "--" + string.Join(", or ", Aliases)
                + (string.IsNullOrEmpty(Delimiter)
                    ? ""
                    : " " + Delimiter)
                    ;
        }
        public override string ToString()
        {
            return Prototype;
        }

        public bool HasAlias(string value)
        {
            return Aliases.Any(alias => value.EqualsIC(alias));
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
    }
}

