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
    public record ArgumentParameter(
        string Prototype,
        IEnumerable<string> Names,
        string? Delimiter = null,
        int? Ordinal = null)
    {
        public string Prototype { get; } = Prototype;
        public int? Ordinal { get; } = Ordinal;

        public static ArgumentParameter Parse(string? value, IFormatProvider? formatProvider)
        {
            if (OrdinalParameter.TryParse(value, formatProvider, out var ordinalParameter))
                return ordinalParameter!;
            if (OptionParameter.TryParse(value, out var optionParameter))
                return optionParameter!;
            if (VisualStudioParameter.TryParse(value, out var visualStudioParameter))
                return visualStudioParameter!;
            throw new ArgumentOutOfRangeException(value);
        }

        public ICollection<string> Aliases { get; } = Names.ToArray();
        public string? Delimiter { get; } = Delimiter;
        public string Help()
        {
            return
                $"--{string.Join(", or ", Aliases)}{(string.IsNullOrEmpty(Delimiter) ? "" : " " + Delimiter)}";
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

        public string? LongAlias()
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

