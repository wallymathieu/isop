using System;

namespace Isop.Infrastructure;
internal static class StringExtensions
{
    /// <summary>
    /// ignore case
    /// </summary>
    /// <returns></returns>
    public static bool EqualsIgnoreCase(this string? self, string? other)
    {
        if (self is null && other is null) return true;
        if (other is null) return false;
        if (self is null) return false;
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
}
