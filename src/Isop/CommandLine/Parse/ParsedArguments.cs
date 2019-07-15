using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isop.Domain;
using System;
namespace Isop.CommandLine.Parse
{
    public abstract class ParsedArguments
    {
        private ParsedArguments(IReadOnlyCollection<RecognizedArgument> recognized, 
            IReadOnlyCollection<UnrecognizedArgument> unrecognized, 
            IReadOnlyCollection<Argument> globalArguments)
        {
            Recognized = recognized;
            Unrecognized = unrecognized;
            GlobalArguments = globalArguments;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parsedArguments"></param>
        private ParsedArguments(ParsedArguments parsedArguments)
        {
            GlobalArguments = parsedArguments.GlobalArguments;
            Unrecognized = parsedArguments.Unrecognized;
            Recognized = parsedArguments.Recognized;
        }
        /// <summary>
        /// Recognized arguments
        /// </summary>
        public IReadOnlyCollection<RecognizedArgument> Recognized { get; }

        /// <summary>
        /// Unrecognized arguments
        /// </summary>
        public IReadOnlyCollection<UnrecognizedArgument> Unrecognized { get; }

        public IReadOnlyCollection<Argument> GlobalArguments { get; }

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
            var unMatchedRequiredArguments = GlobalArguments
                .Where(argumentWithOptions => argumentWithOptions.Required)
                .Where(argumentWithOptions => !Recognized
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
                    globalArguments:first.GlobalArguments.Union(second.GlobalArguments).ToArray(),
                    recognized:first.Recognized.Union(second.Recognized).ToArray(),
                    unrecognized:first.Unrecognized.Intersect(second.Unrecognized).ToArray()
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
            public Default(IEnumerable<RecognizedArgument> recognizedArguments, IEnumerable<UnrecognizedArgument> unrecognizedArguments, IEnumerable<Argument> globalArguments) 
                : base(recognizedArguments.ToArray(), unrecognizedArguments.ToArray(), globalArguments.ToArray())
            {
            }
        }

        /// <summary>
        /// Map from <see cref="ParsedArguments"/> to <see cref="T"/>.
        /// </summary>
        public T Select<T>(Func<Method, T> method, Func<Merged, T> merged, Func<Default, T> @default)
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
    }
}

