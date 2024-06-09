using System;
using System.Linq;
using System.Collections.Generic;
using Isop.Infrastructure;
using Isop.CommandLine.Parse.Parameters;

namespace Isop.CommandLine.Parse;
/// <summary>
/// Represents the parameter. For instance "file" of the commandline argument --file. 
/// </summary>
public class ArgumentParameter(
    string prototype,
    IEnumerable<string> names,
    string? delimiter = null,
    int? ordinal = null)
{
    public string Prototype { get; } = prototype;
    public int? Ordinal { get; } = ordinal;

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

    public ICollection<string> Aliases { get; } = names.ToArray();
    public string? Delimiter { get; } = delimiter;
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

    public string LongAlias()
    {
        var maxLength = Aliases.Max(a => a.Length);
        return Aliases.SingleOrDefault(a => a.Length == maxLength)!;
    }

    public bool Accept(string value)
    {
        return HasAlias(value);
    }
}


