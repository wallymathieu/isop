using System;
using System.Text.RegularExpressions;

namespace Isop
{
    public class OrdinalParameter 
    {
        private static readonly Regex Pattern = new Regex(@"#(?<ord>\d*)(?<rest>.*)");
        public static bool TryParse(string value, IFormatProvider formatProvider, out ArgumentParameter ordinalParameter)
        {
            var match = Pattern.Match(value);
            if (match.Success)
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

