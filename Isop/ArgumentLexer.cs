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
            :this(Lex(arg.ToList()))
        {
        }
        public ArgumentLexer(IEnumerable<Token> tokens)
            :base(tokens)
        {
        }

        public static IEnumerable<Token> Lex(IList<string> arg)
        {
            var currentIndex = 0;
            var length = arg.Count();
            while (currentIndex<length) 
            {
                var value = arg[currentIndex];
                var valueIndex = currentIndex;
                currentIndex++;
                
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
                        if (currentIndex < length)
                        {
                            var possibleParamValue = arg[currentIndex];
                            var possibleParamValueIndex = currentIndex;
                            if (!ParamPattern.IsMatch(possibleParamValue))
                            {
                                currentIndex++;
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
    
}
