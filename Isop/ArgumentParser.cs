using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
namespace Isop
{
    public class ArgumentParser
    {
        private readonly IEnumerable<ArgumentWithOptions> _argumentWithOptions;

        public ArgumentParser(IEnumerable<ArgumentWithOptions> argumentWithOptions)
        {
            _argumentWithOptions = argumentWithOptions;
        }
        public ParsedArguments Parse(IEnumerable<string> arguments)
        {
            var lexer = new ArgumentLexer(arguments);
            var parsedArguments = Parse(lexer, arguments);
            var unMatchedRequiredArguments = parsedArguments.UnMatchedRequiredArguments();

            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments")
                {
                    Arguments = unMatchedRequiredArguments
                        .Select(unmatched => new KeyValuePair<string, string>(unmatched.Argument.ToString(), unmatched.Argument.Help())).ToList()
                };
            }
            return parsedArguments;
        }

        public ParsedArguments Parse(ArgumentLexer lex, IEnumerable<string> arguments)
        {
            var recognizedIndexes = new List<int>();
            var lexer = new PeekEnumerable<Token>(lex);
            IList<RecognizedArgument> recognized = new List<RecognizedArgument>();
            while (lexer.HasMore())
            {
                var current = lexer.Next();
                switch (current.TokenType)
                {
                    case TokenType.Argument:
                        {
                            var argumentWithOptions = _argumentWithOptions
                               .SingleOrDefault(argopt => argopt.Argument.Accept(current.Index,current.Value));
                            
                            if (null == argumentWithOptions)
                            {
                                    continue;
                            }
                            
                            recognizedIndexes.Add(current.Index);
                            recognized.Add(new RecognizedArgument(
                                        argumentWithOptions,
                                        current.Value));
                        }
                        break;
                    case TokenType.Parameter:
                        {
                            var argumentWithOptions = _argumentWithOptions
                               .SingleOrDefault(argopt => argopt.Argument.Accept(current.Index,current.Value));
                            if (null == argumentWithOptions)
                                continue;
                            string value;
                            recognizedIndexes.Add(current.Index);
                            if (lexer.Peek().TokenType == TokenType.ParameterValue)
                            {
                                var paramValue = lexer.Next();
                                recognizedIndexes.Add(paramValue.Index);
                                value = paramValue.Value;
                            }
                            else
                            {
                                value = string.Empty;
                            }

                            recognized.Add(new RecognizedArgument(
                                        argumentWithOptions,
                                        current.Value,
                                        value));
                        }
                        break;
                    case TokenType.ParameterValue:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(current.TokenType.ToString());
                }
            }

            var argumentList = arguments.ToList();

            var unRecognizedArguments = argumentList
                .Select((value, i) => new { i, value })
                .Where(indexAndValue => !recognizedIndexes.Contains(indexAndValue.i))
                .Select(v => new UnrecognizedArgument { Index = v.i, Value = v.value });

            return new ParsedArguments
            {
                ArgumentWithOptions = _argumentWithOptions.ToArray(),
                RecognizedArguments = recognized,
                UnRecognizedArguments = unRecognizedArguments
            };
        }
    }
}
