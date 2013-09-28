using System;
using System.Linq;
using System.Collections.Generic;
using Isop.Infrastructure;
using Isop.Lex;

namespace Isop.Parse
{
    public class ArgumentParser
    {
        private readonly IEnumerable<ArgumentWithOptions> _argumentWithOptions;
        private readonly bool _allowInferParameter;

        public ArgumentParser(IEnumerable<ArgumentWithOptions> argumentWithOptions, bool allowInferParameter)
        {
            _argumentWithOptions = argumentWithOptions;
            _allowInferParameter = allowInferParameter;
        }

        public ParsedArguments Parse(IEnumerable<string> arguments)
        {
            var lexer = ArgumentLexer.Lex(arguments).ToList();
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

        public ParsedArguments Parse(IList<Token> lex, IEnumerable<string> arguments)
        {
            var recognizedIndexes = new List<int>();
            var lexer = new PeekEnumerable<Token>(lex);
            var encounteredParameter = false;
            IList<RecognizedArgument> recognized = new List<RecognizedArgument>();
            while (lexer.HasMore())
            {
                var current = lexer.Next();
                switch (current.TokenType)
                {
                    case TokenType.Argument:
                        {
                            var argumentWithOptions = _argumentWithOptions
                               .SingleOrDefault(argopt => argopt.Argument.Accept(current.Index, current.Value));

                            if (null == argumentWithOptions && !encounteredParameter && _allowInferParameter)
                            {
                                InferParameter(recognizedIndexes, recognized, current);
                                continue;
                            }

                            if (null == argumentWithOptions)
                            {
                                continue;
                            }

                            recognizedIndexes.Add(current.Index);
                            recognized.Add(new RecognizedArgument(
                                        argumentWithOptions,
                                        current.Index,
                                        current.Value));
                        }
                        break;
                    case TokenType.Parameter:
                        {
                            encounteredParameter = true;
                            var argumentWithOptions = _argumentWithOptions
                               .SingleOrDefault(argopt => argopt.Argument.Accept(current.Index, current.Value));
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
                                        current.Index,
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

        private void InferParameter(List<int> recognizedIndexes, IList<RecognizedArgument> recognized, Token current)
        {
            ArgumentWithOptions argumentWithOptions;
            argumentWithOptions = _argumentWithOptions
                .Where((argopt, i) => i == current.Index).SingleOrDefault();
            if (null != argumentWithOptions)
            {
                recognizedIndexes.Add(current.Index);
                recognized.Add(new RecognizedArgument(
                                   argumentWithOptions,
                                   current.Index,
                                   argumentWithOptions.Argument.LongAlias(),
                                   current.Value) {InferredOrdinal = true});
            }
        }
    }
}
