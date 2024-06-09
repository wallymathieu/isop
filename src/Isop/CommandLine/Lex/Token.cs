using System;
using System.Globalization;

namespace Isop.CommandLine.Lex;
public readonly struct Token(string value, TokenType tokenType, int index) : IEquatable<Token>
{
    public string Value { get; } = value;
    public TokenType TokenType { get; } = tokenType;
    /// <summary>
    /// the index in the argument array
    /// </summary>
    public int Index { get; } = index;

    public override readonly bool Equals(object? obj)
    {
        if (obj is null) return false;
        return obj is Token token && Equals(token);
    }
    public static bool operator ==(Token a, Token b) => a.Equals(b);

    public static bool operator !=(Token a, Token b) => !(a == b);

    public readonly bool Equals(Token other)
    {
        return Equals(other.Value, Value) && Equals(other.TokenType, TokenType);
    }

    public override readonly int GetHashCode()
    {
        unchecked
        {
            return ((Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value)) * 397) ^ TokenType.GetHashCode();
        }
    }
    public override readonly string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", TokenType, Value);
    }
}



