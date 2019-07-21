using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Isop.CommandLine.Parse.Parameters
{
    /// <summary>
    /// Visual studio style parameter
    /// </summary>
    public class VisualStudioParameter
    {
        /// <summary>
        /// same pattern as in visual studio external tools: &amp;tool
        /// </summary>
        public static readonly Regex VisualStudioArgPattern = new Regex(@"(?<main>[^=:&]*(?<alias>\&.)?[^=:&]*)(?<equals>[=:])?");

        public static bool TryParse(string value, out ArgumentParameter visualStudioParameter)
        {
            var match = VisualStudioArgPattern.Match(value);
            if (match.Success)
            {
                var main = match.Groups["main"].Value.Replace("&", "");

                var aliases = new List<string>() { main };
                if (match.Groups["alias"].Success)
                {
                    aliases.Add(match.Groups["alias"].Value.Substring(1));
                }
                string delimiter;
                if (match.Groups["equals"].Success)
                {
                    delimiter = match.Groups["equals"].Value;
                }
                else delimiter = null;

                visualStudioParameter = new ArgumentParameter(
                        prototype: value,
                        names: aliases.ToArray(),
                        delimiter: delimiter);

                return true;
            }
            visualStudioParameter = null;
            return false;
        }
    }
}

