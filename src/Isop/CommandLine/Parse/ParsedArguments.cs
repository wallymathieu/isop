using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isop.Domain;
using System;
namespace Isop.CommandLine.Parse
{
    public abstract class ParsedArguments
    {
        private ParsedArguments(IReadOnlyCollection<RecognizedArgument> recognizedArguments, IReadOnlyCollection<UnrecognizedArgument> unRecognizedArguments, IReadOnlyCollection<Argument> argumentWithOptions)
        {
            RecognizedArguments = recognizedArguments;
            UnRecognizedArguments = unRecognizedArguments;
            ArgumentWithOptions = argumentWithOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parsedArguments"></param>
        private ParsedArguments(ParsedArguments parsedArguments)
        {
            ArgumentWithOptions = parsedArguments.ArgumentWithOptions;
            UnRecognizedArguments = parsedArguments.UnRecognizedArguments;
            RecognizedArguments = parsedArguments.RecognizedArguments;
        }
        public IReadOnlyCollection<RecognizedArgument> RecognizedArguments { get; }

        public IReadOnlyCollection<UnrecognizedArgument> UnRecognizedArguments { get; }

        public IReadOnlyCollection<Argument> ArgumentWithOptions { get; }

        public ParsedArguments Merge(ParsedArguments args)
        {
            return Merge(this, args);
        }

        private static ParsedArguments Merge(ParsedArguments first, ParsedArguments second)
        {
            return new Merged(first, second);
        }

        public IEnumerable<Argument> UnMatchedRequiredArguments()
        {
            var unMatchedRequiredArguments = ArgumentWithOptions
                .Where(argumentWithOptions => argumentWithOptions.Required)
                .Where(argumentWithOptions => !RecognizedArguments
                                                   .Any(recogn => recogn.Argument.Equals(argumentWithOptions)));
            return unMatchedRequiredArguments;
        }

        public void AssertFailOnUnMatched()
        {
            var unMatchedRequiredArguments = UnMatchedRequiredArguments();

            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments")
                          {
                              Arguments = unMatchedRequiredArguments
                                  .Select(unmatched =>unmatched.Name)
                                  .ToArray()
                          };
            }
        }
        public IEnumerable<KeyValuePair<string,string>> RecognizedArgumentsAsKeyValuePairs(){
            return RecognizedArguments.Select(a => a.AsKeyValuePair());
        }
        
        /// <summary>
        /// A combination of two parsed arguments instances
        /// </summary>
        public class Merged : ParsedArguments
        {
            /// <summary>
            /// 
            /// </summary>
            public readonly ParsedArguments First;
            /// <summary>
            /// 
            /// </summary>
            public readonly ParsedArguments Second;

            internal Merged(ParsedArguments first, ParsedArguments second) 
                : base(
                    argumentWithOptions:first.ArgumentWithOptions.Union(second.ArgumentWithOptions).ToArray(),
                    recognizedArguments:first.RecognizedArguments.Union(second.RecognizedArguments).ToArray(),
                    unRecognizedArguments:first.UnRecognizedArguments.Intersect(second.UnRecognizedArguments).ToArray()
                )
            {
                First = first;
                Second = second;
            }
        }
        public class Method : ParsedArguments
        {
            public Method(ParsedArguments parsedArguments,
                Type recognizedClass,
                Domain.Method recognizedAction,
                IEnumerable<object> recognizedActionParameters)
                : base(parsedArguments)
            {
                RecognizedClass = recognizedClass;
                RecognizedAction = recognizedAction;
                RecognizedActionParameters = recognizedActionParameters.ToArray();
            }

            public Type RecognizedClass { get; }
            public Domain.Method RecognizedAction { get; }
            public IReadOnlyCollection<object> RecognizedActionParameters { get; }
        }

        public class Default : ParsedArguments
        {
            public Default(IEnumerable<RecognizedArgument> recognizedArguments, IEnumerable<UnrecognizedArgument> unRecognizedArguments, IEnumerable<Argument> argumentWithOptions) 
                : base(recognizedArguments.ToArray(), unRecognizedArguments.ToArray(), argumentWithOptions.ToArray())
            {
            }
        }

        public T Map<T>(Func<Method, T> method, Func<Merged, T> merged, Func<Default, T> @default)
        {
            switch (this)
            {
                case Method pm: return method(pm);
                case Merged m: return merged(m);
                case Default d: return @default(d);
                default:
                    throw new Exception("Unimplemented switch case");
            }
        }
        public void Switch(Action<Method> method, Action<Merged> merged, Action<Default> @default)
        {
            switch (this)
            {
                case Method pm: 
                    method(pm);
                    return;
                case Merged m: 
                    merged(m);
                    return;
                case Default d: 
                    @default(d);
                    return;
                default:
                    throw new Exception("Unimplemented switch case");
            }
        }
    }
}

