using System;
using System.Text.RegularExpressions;

namespace Isop.Infrastructure
{
    internal static class StringExtensions
    {
        /// <summary>
        /// ignore case
        /// </summary>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string self, string other)
        {
            return self.Equals(other, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// ignore case
        /// </summary>
        /// <returns></returns>
        public static bool StartsWithIgnoreCase(this string self, string other)
        {
            return self.StartsWith(other, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// ignore case
        /// </summary>
        /// <returns></returns>
        public static bool EndsWithIgnoreCase(this string self, string other)
        {
            return self.EndsWith(other, StringComparison.OrdinalIgnoreCase);
        }
    }
}
