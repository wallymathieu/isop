using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Isop.CommandLine.Parse.Parameters
{
    public class OptionParameter 
    {
        private static readonly List<char> delimiters = ['=', ':'];
        /// <summary>
        /// Note: this may accept invalid patterns.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="optionParameter"></param>
        /// <returns></returns>
        public static bool TryParse(
            string? value,
            #if NET8_0_OR_GREATER
            [NotNullWhen(true)]
            #endif
            out ArgumentParameter? optionParameter)
        {
            if (value != null && value.Contains("|"))
            {
                var prototype = value;
                var names = prototype.TrimEnd(delimiters.ToArray()).Split('|');
                string? delimiter = null;
                var last = prototype.Last();
                if (delimiters.Contains(last))
                    delimiter = last.ToString();
                optionParameter = new ArgumentParameter(prototype, names, delimiter);
                return true;
            }
            optionParameter = null;
            return false;
        }
    }
}

