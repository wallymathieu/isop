
namespace Isop.Abstractions;

/// <summary>
/// Represents a type converter
/// </summary>
public delegate object? TypeConverter(System.Type type, string? value, System.Globalization.CultureInfo? culture);
