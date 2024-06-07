using System;
using System.Linq;
using System.Collections.Generic;

namespace Isop.CommandLine.Parse
{
    using Infrastructure;
    using Lex;

    public class ArgumentParser(
        IReadOnlyCollection<Argument> globalArguments, 
        bool allowInferParameter)
    {

        public ParsedArguments.Properties Parse(IReadOnlyList<Token> lexed, IReadOnlyCollection<string> arguments)
        {
            var peekTokens = new PeekEnumerable<Token>(lexed);
            var encounteredParameter = false;
            IList<RecognizedArgument> recognized = [];
            while (peekTokens.HasMore())
            {
                var current = peekTokens.Next();
                switch (current.TokenType)
                {
                    case TokenType.Argument:
                        {
                            var argument = globalArguments
                               .SingleOrDefault(arg => arg.Accept(current.Index, current.Value));

                            if (null == argument && !encounteredParameter && allowInferParameter)
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
                            var argumentWithOptions = globalArguments
                                .SingleOrDefault(argopt => argopt.Accept(current.Index, current.Value));
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
                        throw new Exception($"Unexpected token type {current.TokenType}");
                }
            }

            // Inferred ordinal arguments should not be recognized twice
            var minusDuplicates=
                recognized
                    .Where(argument =>
                        !recognized.Any(otherArgument =>
                            argument.InferredOrdinal &&
                            !ReferenceEquals(argument, otherArgument) 
                            && (otherArgument.RawArgument?.Equals(argument.RawArgument)??false)))
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
            var argumentWithOptions = globalArguments
                .Where((argopt, i) => i == current.Index).SingleOrDefault();
            if (null != argumentWithOptions)
            {
                recognized.Add(new RecognizedArgument(
                                   argument: argumentWithOptions,
                                   index: [current.Index],
                                   rawArgument: argumentWithOptions.Name,
                                   value: current.Value,
                                   inferredOrdinal:true));
            }
        }
    }
}
