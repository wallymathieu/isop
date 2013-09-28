using System;

namespace Isop.Infrastructure
{
    public static class StringExtensions
    {
        /// <summary>
        /// ignore case
        /// </summary>
        /// <returns></returns>
        public static bool EqualsIC(this string self, string other)
        {
            return self.Equals(other, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// ignore case
        /// </summary>
        /// <returns></returns>
        public static bool StartsWithIC(this string self, string other)
        {
            return self.StartsWith(other, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// ignore case
        /// </summary>
        /// <returns></returns>
        public static bool EndsWithIC(this string self, string other)
        {
            return self.EndsWith(other, StringComparison.OrdinalIgnoreCase);
        }

        public static string RemoveSetFromBeginningOfString(this string arg)
        {
            if (arg.StartsWithIC("set_"))
                return arg.Substring(4);
            if (arg.StartsWithIC("set"))
                return arg.Substring(3);
            return arg;
        }
    }
}
