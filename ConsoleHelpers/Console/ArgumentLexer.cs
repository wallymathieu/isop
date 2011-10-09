using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Helpers.Console
{
    public class ArgumentLexer : IEnumerable<Token>//TODO: IEnumerator<Token>
    {
        private static readonly Regex ParamPattern = new Regex("(?<paramPrefix>--|/|-)(?<param>[^:=]*)([:=]?)(?<paramValue>.*)");
        private int _currentIndex = -1;
        private List<Token> _buffer;
        private static readonly Token None = new Token("None", TokenType.None, -1);

        public ArgumentLexer(IEnumerable<string> arg)
            :this(Lex(arg.ToList()))
        {
        }
        public ArgumentLexer(IEnumerable<Token> tokens)
        {
            _buffer = new List<Token>(tokens);
        }

        public bool HasMore() { return _currentIndex+1<_buffer.Count(); }

        public Token Current()
        {
            if (_currentIndex < _buffer.Count())
            {
                return _buffer[_currentIndex];
            }
            throw new ArgumentOutOfRangeException("_currentIndex >= ");
        }

        private static IEnumerable<Token> Lex(IList<string> _arg)
        {
            int _currentIndex = 0;
            int length = _arg.Count();
            while (_currentIndex<length) 
            {
                var value = _arg[_currentIndex];
                var valueIndex = _currentIndex;
                _currentIndex++;
                
                Match match = ParamPattern.Match(value);
                if (match.Success)
                {
                    yield return new Token(match.Groups["param"].Value, TokenType.Parameter, valueIndex);
                    if (match.Groups["paramValue"].Length > 0)
                    {
                        yield return new Token(match.Groups["paramValue"].Value, TokenType.ParameterValue, valueIndex);
                    }
                    else
                    {
                        if (_currentIndex < length)
                        {
                            var possibleParamValue = _arg[_currentIndex];
                            var possibleParamValueIndex = _currentIndex;
                            if (!ParamPattern.IsMatch(possibleParamValue))
                            {
                                _currentIndex++;
                                yield return new Token(possibleParamValue, TokenType.ParameterValue, possibleParamValueIndex);
                            }
                        }
                    }
                }
                else
                {
                    yield return new Token(value, TokenType.Argument, valueIndex);
                }
            }
        }

        public Token Next()
        {
            _currentIndex++;
            return Current();
        }

        public Token Peek()
        {
             var idx = _currentIndex+1; 
             if (idx<_buffer.Count())
                 return _buffer[idx];
             return None;
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return this._buffer.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    public enum TokenType
    {
        Argument,
        Parameter,
        ParameterValue,
        None
    }
    public class Token
    {
        public string Value { get; private set; }
        public TokenType TokenType { get; private set; }
        /// <summary>
        /// the index in the argument array
        /// </summary>
        public int Index { get; private set; }

        public Token(string value, TokenType tokenType, int index)
        {
            Value = value;
            TokenType = tokenType;
            Index = index;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Token)) return false;
            return Equals((Token)obj);
        }

        public bool Equals(Token other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
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
            return string.Format("{0}:{1}", TokenType, Value);
        }
    }
}
