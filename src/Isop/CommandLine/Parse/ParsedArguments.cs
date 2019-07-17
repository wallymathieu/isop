using System.Collections.Generic;
using System.Linq;
using System;
namespace Isop.CommandLine.Parse
{
    public abstract class ParsedArguments
    {
        private ParsedArguments(IReadOnlyCollection<RecognizedArgument> recognized, 
            IReadOnlyCollection<UnrecognizedArgument> unrecognized)
        {
            Recognized = recognized;
            Unrecognized = unrecognized;
        }

        /// <summary>
        /// Recognized arguments
        /// </summary>
        public IReadOnlyCollection<RecognizedArgument> Recognized { get; }

        /// <summary>
        /// Unrecognized arguments
        /// </summary>
        public IReadOnlyCollection<UnrecognizedArgument> Unrecognized { get; }
        
        public ParsedArguments Merge(ParsedArguments args)
        {
            return Merge(this, args);
        }

        private static ParsedArguments Merge(ParsedArguments first, ParsedArguments second)
        {
            return new Merged(first, second);
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
                    unrecognized:first.Unrecognized.Intersect(second.Unrecognized).ToArray(),
                    recognized:first.Recognized.Union(second.Recognized).ToArray()
                )
            {
                First = first;
                Second = second;
            }
        }
        public class Method : ParsedArguments
        {
            public Method(Type recognizedClass,
                Domain.Method recognizedAction,
                IEnumerable<object> recognizedActionParameters)
                : base(new RecognizedArgument[0],new UnrecognizedArgument[0])
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
            public Default(IEnumerable<RecognizedArgument> recognizedArguments, IEnumerable<UnrecognizedArgument> unrecognizedArguments) 
                : base(recognizedArguments.ToArray(), unrecognizedArguments.ToArray())
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

