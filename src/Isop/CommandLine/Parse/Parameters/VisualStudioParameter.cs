using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System;

namespace Isop.CommandLine.Parse.Parameters;
/// <summary>
/// Visual studio style parameter
/// </summary>
public static class VisualStudioParameter
{
    /// <summary>
    /// same pattern as in visual studio external tools: &amp;tool
    /// </summary>
    public static readonly Regex VisualStudioArgPattern = new(@"(?<main>[^=:&]*(?<alias>\&.)?[^=:&]*)(?<equals>[=:])?", RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));

    public static bool TryParse(string? value,
#if NET8_0_OR_GREATER
            [NotNullWhen(true)]
#endif
        out ArgumentParameter? visualStudioParameter)
    {
        Match match;
        if (value != null
            && (match = VisualStudioArgPattern.Match(value)).Success)
        {
            var main = match.Groups["main"].Value.Replace("&", "");

            var aliases = new List<string>() { main };
            if (match.Groups["alias"].Success)
            {
                aliases.Add(match.Groups["alias"].Value.Substring(1));
            }
            string? delimiter;
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


