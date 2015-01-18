using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Isop.CommandLine.Parse.Parameters
{
    public class VisualStudioParameter 
    {
        /// <summary>
        /// same pattern as in visual studio external tools: &amp;tool
        /// </summary>
        public static readonly Regex VisualStudioArgPattern = new Regex(@"(?<prefix>\&?)(?<alias>.)[^=:]*(?<equals>[=:]?)");

        public static bool TryParse(string value, out ArgumentParameter visualStudioParameter)
        {
            //TODO: need to do some cleaning here
            var match = VisualStudioArgPattern.Match(value);
            if (match.Success)
            {
                var aliases = new List<string>();
                string val;
                if (match.Groups["prefix"].Length > 0)
                {
                    val = value.Replace("&", "");
                    if (match.Groups["alias"].Length > 0)
                        aliases.Add(match.Groups["alias"].Value);
                }
                else
                {
                    val = value;
                }
                string delimiter;
                if (match.Groups["equals"].Length > 0)
                {
                    delimiter = match.Groups["equals"].Value;
                    val = val.Replace(delimiter, "");
                }
                else delimiter = null;
                aliases.Add(val);

                visualStudioParameter = new ArgumentParameter(
                        prototype:value, 
                        names:aliases.ToArray(), 
                        delimiter:delimiter);
                
                return true;
            }
            visualStudioParameter = null;
            return false;
        }
    }}

