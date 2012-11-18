using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Isop
{
    public class ArgumentLexer : List<Token>
    {
        private static readonly Regex ParamPattern = new Regex("(?<paramPrefix>--|/|-)(?<param>[^:=]*)([:=]?)(?<paramValue>.*)");

        public ArgumentLexer(IEnumerable<string> arg)
            : this(Lex(arg.ToList()))
        {
        }

        public ArgumentLexer(IEnumerable<Token> tokens)
            : base(tokens)
        {
        }

        private static Token mapParametervaluesBasedOnParameters(Token? previous, Token token)
        {
            if (null != previous && previous.Value.TokenType == TokenType.Parameter && token.TokenType == TokenType.Argument)
            {
                return new Token(token.Value, TokenType.ParameterValue, token.Index);
            }
            return token;
        }

        private static IEnumerable<Token> MapTokensFromRaw(string arg, int valueIndex)
        {
            var match = ParamPattern.Match(arg);
            if (match.Success)
            {
                yield return new Token(match.Groups["param"].Value, TokenType.Parameter, valueIndex);
                if (match.Groups["paramValue"].Length > 0)
                {
                    yield return new Token(match.Groups["paramValue"].Value, TokenType.ParameterValue, valueIndex);
                }
            }
            else
            {
                yield return new Token(arg, TokenType.Argument, valueIndex);
            }
        }

        private static IEnumerable<T> SelectWithPrevious<T>(Func<T?, T, T> map, IEnumerable<T> sequence) where T : struct
        {
            T? last = null;
            foreach (T item in sequence)
            {
                yield return map(last, item);
                last = item;
            }
        }

        public static IEnumerable<Token> Lex(IList<string> arg)
        {
            var firstSweep = arg.SelectMany((value, valueIndex) => MapTokensFromRaw(value, valueIndex));
            return SelectWithPrevious<Token>(mapParametervaluesBasedOnParameters, firstSweep);
        }
    }
}
