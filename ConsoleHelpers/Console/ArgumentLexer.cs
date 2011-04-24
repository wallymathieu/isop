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
        private readonly string[] _arg;
        private int _currentIndex = 0;
        private readonly List<Token> _buffer = new List<Token>();
        private static readonly Token None = new Token("None", TokenType.None, -1);

        public ArgumentLexer(IEnumerable<string> arg)
        {
            _arg = arg.ToArray();
        }

        public bool HasMore() { return _buffer.Any() || HasMoreToLex(); }

        private bool HasMoreToLex()
        {
            return _currentIndex < _arg.Length;
        }

        public Token Current()
        {
            if (_buffer.Any())
            {
                return _buffer.First();
            }
            LexMore();
            return _buffer.First();
        }

        private void LexMore()
        {
            if (HasMoreToLex())
            {
                _buffer.AddRange(Lex());
            }
            else
            {
                throw new NothingLeftToLexException("nothing more to lex");
            }
        }

        private IEnumerable<Token> Lex()
        {
            var value = _arg[_currentIndex];
            var valueIndex = _currentIndex;
            _currentIndex++;
            var match = ParamPattern.Match(value);
            if (match.Success)
            {
                yield return new Token(match.Groups["param"].Value, TokenType.Parameter, valueIndex);
                if (match.Groups["paramValue"].Length > 0)
                {
                    yield return new Token(match.Groups["paramValue"].Value, TokenType.ParameterValue, valueIndex);
                }
                else
                {
                    if (HasMoreToLex())
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

        public Token Next()
        {
            if (_buffer.Any())
            {
                return BufferPop();
            }
            LexMore();
            return BufferPop();
        }

        private Token BufferPop()
        {
            var token = _buffer.First();
            _buffer.RemoveAt(0);
            return token;
        }

        public Token Peek()
        {
            try
            {
                Token next = None;
                for (int i = 0; i < 2; i++)
                {
                    next = Next();
                    _buffer.Add(next);
                }
                return next;
            }
            catch (NothingLeftToLexException)
            {
                return None;
            }
        }

        public IEnumerator<Token> GetEnumerator()
        {
            while (HasMore())
            {
                yield return Next();
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class NothingLeftToLexException : Exception
    {
        public NothingLeftToLexException(string msg)
            : base(msg)
        {
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
