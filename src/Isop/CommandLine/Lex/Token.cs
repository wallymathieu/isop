using System.Globalization;

namespace Isop.CommandLine.Lex
{
    public struct Token(string value, TokenType tokenType, int index)
    {
        public string Value = value;
        public TokenType TokenType = tokenType;
        /// <summary>
        /// the index in the argument array
        /// </summary>
        public int Index = index;

        public override readonly bool Equals(object obj)
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
                return ((Value != null ? Value.GetHashCode() : 0) * 397) ^ TokenType.GetHashCode();
            }
        }
        public override readonly string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,"{0}:{1}", TokenType, Value);
        }
    }

}

