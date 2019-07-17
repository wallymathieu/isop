using System;
using System.Linq;
using System.Collections.Generic;

namespace Isop.CommandLine.Parse
{
    using Infrastructure;
    using Lex;

    public class ArgumentParser
    {
        private readonly IReadOnlyCollection<Argument> _globalArguments;
        private readonly bool _allowInferParameter;

        public ArgumentParser(IReadOnlyCollection<Argument> globalArguments, bool allowInferParameter)
        {
            _globalArguments = globalArguments;
            _allowInferParameter = allowInferParameter;
        }

        public ParsedArguments Parse(IReadOnlyList<Token> lexed, IReadOnlyCollection<string> arguments)
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
                            var argumentWithOptions = _globalArguments
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
                            var argumentWithOptions = _globalArguments
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

            var argumentList = arguments.ToList(); //TODO: Should use lexed

            var unRecognizedArguments = argumentList
                .Select((value, i) => new { i, value })
                .Where(indexAndValue => !recognizedIndexes.Contains(indexAndValue.i))
                .Select(v => new UnrecognizedArgument { Index = v.i, Value = v.value });

            return new ParsedArguments.Default(
                unrecognizedArguments : unRecognizedArguments,
                recognizedArguments : recognized
            );
        }

        private bool Accept(Argument argument, int index, string value)
        {
            return argument.Accept(index, value);
        }

        private void InferParameter(ICollection<int> recognizedIndexes, IList<RecognizedArgument> recognized, Token current)
        {
            var argumentWithOptions = _globalArguments
                .Where((argopt, i) => i == current.Index).SingleOrDefault();
            if (null != argumentWithOptions)
            {
                recognizedIndexes.Add(current.Index);
                recognized.Add(new RecognizedArgument(
                                   argumentWithOptions,
                                   current.Index,
                                   argumentWithOptions.Name,
                                   current.Value) { InferredOrdinal = true });
            }
        }

        private bool Accept(Argument argument, string value)
        {
            return argument.Accept(value);
        }
    }
}
