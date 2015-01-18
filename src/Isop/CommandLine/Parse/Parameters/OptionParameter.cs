using System.Linq;

namespace Isop.CommandLine.Parse.Parameters
{
    public class OptionParameter 
    {
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
                var names = prototype.TrimEnd('=', ':').Split('|');
                string delimiter = null;
                var last = prototype.Last();
                switch (last)
                {
                    case '=':
                    case ':':
                        delimiter = last.ToString();
                        break;
                    default:
                        break;
                }
                optionParameter = new ArgumentParameter(prototype, names, delimiter);
                return true;
            }
            optionParameter = null;
            return false;
        }
    }
}

