using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace Isop.Implementations;

internal static class DefaultConverter
{
    public static object? ConvertFrom(Type type, string? s, CultureInfo? cultureInfo)
    {
        if (s is null) return null;
        if (type == typeof(FileStream))
        {
            return new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        return TypeDescriptor.GetConverter(type).ConvertFrom(context: null, cultureInfo, s);
    }
}
