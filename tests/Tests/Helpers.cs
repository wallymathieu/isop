using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests.ArgumentParsers;
internal static class Helpers
{
    public static IEnumerable<KeyValuePair<string, T>> DictionaryDescriptionToKv<T>(string input, Func<string, T> convert)
    {
        var expected = Regex.Matches(input,
                @"\[(?<p>#?\w*), (?<v>\w*)\]")
            .Cast<Match>()
            .Select(m => new KeyValuePair<string, T>(m.Groups["p"].Value, convert(m.Groups["v"].Value)));
        return expected;
    }
}
