using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Isop
{
    public class PeekEnumerable<T>:IEnumerable<T>
    {
        public PeekEnumerable(IEnumerable<T> enumerable)
        {
            _buffer = enumerable.ToList();
        }
        private int _currentIndex = -1;
        private List<T> _buffer;
        public T Current()
        {
            if (_currentIndex < _buffer.Count())
            {
                return _buffer[_currentIndex];
            }
            throw new ArgumentOutOfRangeException("_currentIndex >= count");
        }
        public bool HasMore() { return _currentIndex+1<_buffer.Count(); }
        public T Next()
        {
            _currentIndex++;
            return Current();
        }

        public T Peek()
        {
             var idx = _currentIndex+1; 
             if (idx<_buffer.Count())
                 return _buffer[idx];
             return default(T);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return this._buffer.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
    
    public class ArgumentLexer : List<Token>
    {
        private static readonly Regex ParamPattern = new Regex("(?<paramPrefix>--|/|-)(?<param>[^:=]*)([:=]?)(?<paramValue>.*)");
        
        public ArgumentLexer(IEnumerable<string> arg)
            :this(Lex(arg.ToList()))
        {
        }
        public ArgumentLexer(IEnumerable<Token> tokens)
            :base(tokens)
        {
        }

        public static IEnumerable<Token> Lex(IList<string> _arg)
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
    }
    
    public enum TokenType
    {
        None,
        Argument,
        Parameter,
        ParameterValue,
    }
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
            this.Value = value;
            this.TokenType = tokenType;
            this.Index = index;
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
            return string.Format("{0}:{1}", TokenType, Value);
        }
    }
}
