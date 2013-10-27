using System.Globalization;

namespace Isop.Lex
{
    public struct Token
    {
        public string Value;
        public TokenType TokenType;
        /// <summary>
        /// the index in the argument array
        /// </summary>
        public int Index;
        
        public Token(string value, TokenType tokenType, int index)
        {
            Value = value;
            TokenType = tokenType;
            Index = index;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Token && Equals((Token)obj);
        }

        public bool Equals(Token other)
        {
            return Equals(other.Value, Value) && Equals(other.TokenType, TokenType);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Value != null ? Value.GetHashCode() : 0) * 397) ^ TokenType.GetHashCode();
            }
        }
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,"{0}:{1}", TokenType, Value);
        }
    }

}

