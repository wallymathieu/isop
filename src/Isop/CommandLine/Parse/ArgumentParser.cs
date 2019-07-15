using System;
using System.Linq;
using System.Collections.Generic;
using Isop.Infrastructure;
using Isop.CommandLine.Lex;
using Isop.Domain;


namespace Isop.CommandLine.Parse
{
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

            var argumentList = arguments.ToList();

            var unRecognizedArguments = argumentList
                .Select((value, i) => new { i, value })
                .Where(indexAndValue => !recognizedIndexes.Contains(indexAndValue.i))
                .Select(v => new UnrecognizedArgument { Index = v.i, Value = v.value });

            return new ParsedArguments.Default(
                globalArguments :_globalArguments.ToArray(),
                recognizedArguments : recognized,
                unrecognizedArguments : unRecognizedArguments
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

        public ParsedArguments Parse(Dictionary<string, string> arg)
        {
            var recognized = new List<RecognizedArgument>();
            var unRecognizedArguments = new List<UnrecognizedArgument>();
            var index = 0;
            foreach (var current in arg)
            {
                var argumentWithOptions = _globalArguments
                        .SingleOrDefault(argopt => Accept(argopt, current.Key));


                if (null == argumentWithOptions)
                {
                    unRecognizedArguments.Add(new UnrecognizedArgument { Value = current.Key, Index = index++ });
                    continue;
                }
                recognized.Add(new RecognizedArgument(
                            argumentWithOptions,
                            index++,
                            current.Key,
                            current.Value));

            }
            return new ParsedArguments.Default(
                globalArguments : _globalArguments.ToArray(),
                recognizedArguments : recognized,
                unrecognizedArguments : unRecognizedArguments
            );

        }

        private bool Accept(Argument argument, string value)
        {
            return argument.Accept(value);
        }
    }
}
