using System;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace Isop.CommandLine.Parse.Parameters
{
    public class OrdinalParameter 
    {
        private static readonly Regex Pattern = new(@"#(?<ord>\d+)(?<rest>.*)");
        public static bool TryParse(
            string? value, 
            IFormatProvider? formatProvider,
            #if NET8_0_OR_GREATER
            [NotNullWhen(true)]
            #endif
            out ArgumentParameter? ordinalParameter)
        {
            Match match;
            if (value!=null 
                && (match = Pattern.Match(value)).Success)
            {
                var prototype = value;
                var rest = match.Groups["rest"].Value;
                var param = ArgumentParameter.Parse(rest, formatProvider);
                ordinalParameter = new ArgumentParameter(prototype, param.Aliases, param.Delimiter, int.Parse(match.Groups["ord"].Value, formatProvider));
                return true;
            }
            ordinalParameter = null;
            return false;
        }
    }
}

