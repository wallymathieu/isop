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

        public ParsedArguments.Properties Parse(IReadOnlyList<Token> lexed, IReadOnlyCollection<string> arguments)
        {
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
                            var argument = _globalArguments
                               .SingleOrDefault(arg => Accept(arg, current.Index, current.Value));

                            if (null == argument && !encounteredParameter && _allowInferParameter)
                            {
                                InferParameter(recognized, current);
                                continue;
                            }

                            if (null == argument)
                            {
                                continue;
                            }

                            recognized.Add(new RecognizedArgument(
                                        argument,
                                        new []{current.Index},
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
                            var indexes = new List<int> {current.Index};
                            if (peekTokens.Peek().TokenType == TokenType.ParameterValue)
                            {
                                var paramValue = peekTokens.Next();
                                indexes.Add(paramValue.Index);
                                value = paramValue.Value;
                            }
                            else
                            {
                                value = string.Empty;
                            }

                            recognized.Add(new RecognizedArgument(
                                        argumentWithOptions,
                                        indexes.ToArray(),
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

            // Inferred ordinal arguments should not be recognized twice
            var minusDuplicates=
                recognized
                    .Where(argument =>
                        !recognized.Any(otherArgument =>
                            argument.InferredOrdinal &&
                            !ReferenceEquals(argument, otherArgument) 
                            && otherArgument.RawArgument.Equals(argument.RawArgument)))
                .ToList();
            var recognizedIndexes = minusDuplicates.SelectMany(token=>token.Index).ToList();

            var unRecognizedArguments = arguments
                .Select((value, i) => new { Index = i, Value = value })
                .Where(indexAndValue => !recognizedIndexes.Contains(indexAndValue.Index))
                .Select(v => new UnrecognizedArgument(v.Index, v.Value));

            return new ParsedArguments.Properties(
                unrecognized: unRecognizedArguments.ToArray(),
                recognized : minusDuplicates.ToArray()
            );
        }

        private bool Accept(Argument argument, int index, string value)
        {
            return argument.Accept(index, value);
        }

        private void InferParameter(IList<RecognizedArgument> recognized, Token current)
        {
            var argumentWithOptions = _globalArguments
                .Where((argopt, i) => i == current.Index).SingleOrDefault();
            if (null != argumentWithOptions)
            {
                recognized.Add(new RecognizedArgument(
                                   argumentWithOptions,
                                   new []{current.Index},
                                   argumentWithOptions.Name,
                                   current.Value,
                                   inferredOrdinal:true));
            }
        }
    }
}
