using System.Collections.Generic;
using System.Linq;

namespace Isop.CommandLine.Parse.Parameters
{
    public class OptionParameter 
    {
        private static List<char> delimiters = new List<char> { '=', ':' };
        /// <summary>
        /// Note: this may accept invalid patterns.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="optionParameter"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out ArgumentParameter optionParameter)
        {
            if (value.Contains("|"))
            {
                var prototype = value;
                var names = prototype.TrimEnd(delimiters.ToArray()).Split('|');
                string delimiter = null;
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

