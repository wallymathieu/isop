using System;
using System.Linq;
using System.Collections.Generic;
using Isop.Infrastructure;
using Isop.CommandLine.Lex;
using Isop.Domain;
using System.Globalization;


namespace Isop.CommandLine.Parse
{
    public class ArgumentParser
    {
        private readonly IEnumerable<Argument> _argumentWithOptions;
        private readonly bool _allowInferParameter;
        private readonly CultureInfo cultureInfo;

        public ArgumentParser(IEnumerable<Argument> argumentWithOptions, bool allowInferParameter, CultureInfo cultureInfo)
        {
            _argumentWithOptions = argumentWithOptions;
            _allowInferParameter = allowInferParameter;
            this.cultureInfo = cultureInfo; 
        }

        public ParsedArguments Parse(IEnumerable<string> arguments)
        {
            var lexed = ArgumentLexer.Lex(arguments).ToList();
            var parsedArguments = Parse(lexed, arguments);
            var unMatchedRequiredArguments = parsedArguments.UnMatchedRequiredArguments();

            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments")
                {
                    Arguments = unMatchedRequiredArguments
                        .Select(unmatched => unmatched.Name).ToArray()
                };
            }
            return parsedArguments;
        }

        public ParsedArguments Parse(IList<Token> lexed, IEnumerable<string> arguments)
        {
            var recognizedIndexes = new List<int>();
            var peekTokens = new PeekEnumerable<Token>(lexed);
            var encounteredParameter = false;
            IList<RecognizedArgument> recognized = new List<RecognizedArgument>();
            while (peekTokens.HasMore())
            {
                var current = peekTokens.Next();
                switch (current.TokenType)
                {
                    case TokenType.Argument:
                        {
                            var argumentWithOptions = _argumentWithOptions
                               .SingleOrDefault(argopt => Accept(argopt, current.Index, current.Value));

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
                                .SingleOrDefault(argopt => Accept(argopt, current.Index, current.Value));
                            if (null == argumentWithOptions)
                                continue;
                            string value;
                            recognizedIndexes.Add(current.Index);
                            if (peekTokens.Peek().TokenType == TokenType.ParameterValue)
                            {
                                var paramValue = peekTokens.Next();
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

            return new ParsedArguments(argumentList)
            {
                ArgumentWithOptions = _argumentWithOptions.ToArray(),
                RecognizedArguments = recognized,
                UnRecognizedArguments = unRecognizedArguments
            };
        }

        private bool Accept(Argument argument, int index, string value){
            if (argument is ArgumentWithOptions)
            {
                return ((ArgumentWithOptions)argument).Argument.Accept(index, value);
            }
            return ArgumentParameter.Parse(argument.Name, cultureInfo).Accept(index, value);
        }

        private void InferParameter(List<int> recognizedIndexes, IList<RecognizedArgument> recognized, Token current)
        {
            Argument argumentWithOptions;
            argumentWithOptions = _argumentWithOptions
                .Where((argopt, i) => i == current.Index).SingleOrDefault();
            if (null != argumentWithOptions)
            {
                recognizedIndexes.Add(current.Index);
                recognized.Add(new RecognizedArgument(
                                   argumentWithOptions,
                                   current.Index,
                                   argumentWithOptions.Name,
                                   current.Value) {InferredOrdinal = true});
            }
        }
    }
}
