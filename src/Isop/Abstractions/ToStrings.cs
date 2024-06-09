namespace Isop.Abstractions;

/// <summary>
/// Do a ToString conversion to format a value as a string. To format enumerable as multiple strings. 
/// </summary>
public delegate IEnumerable<string> ToStrings(object? value);

