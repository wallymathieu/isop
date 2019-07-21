using System.Collections.Generic;
using System.Linq;
using System;
namespace Isop.CommandLine.Parse
{
    public abstract class ParsedArguments
    {
        public ParsedArguments Merge(ParsedArguments args)
        {
            return new Merged(this, args);
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
            {
                First = first;
                Second = second;
            }
        }

        public class MethodMissingArguments : ParsedArguments
        {
            public Type RecognizedClass { get; }
            public Domain.Method RecognizedAction { get; }
            public IReadOnlyCollection<string> MissingParameters { get; }

            public MethodMissingArguments(Type recognizedClass,
                Domain.Method recognizedAction,
                IReadOnlyCollection<string> missingParameters)
            {
                RecognizedClass = recognizedClass;
                RecognizedAction = recognizedAction;
                MissingParameters = missingParameters;
            }
        }

        public class Method : ParsedArguments
        {
            public Method(Type recognizedClass,
                Domain.Method recognizedAction,
                IEnumerable<object> recognizedActionParameters,
                IEnumerable<RecognizedArgument> recognized)
            {
                RecognizedClass = recognizedClass;
                RecognizedAction = recognizedAction;
                Recognized = recognized;
                RecognizedActionParameters = recognizedActionParameters.ToArray();
            }

            public Type RecognizedClass { get; }
            public Domain.Method RecognizedAction { get; }
            public IEnumerable<RecognizedArgument> Recognized { get; }
            public IReadOnlyCollection<object> RecognizedActionParameters { get; }
        }

        public class Properties : ParsedArguments
        {
            /// <summary>
            /// Recognized arguments
            /// </summary>
            public IReadOnlyCollection<RecognizedArgument> Recognized { get; }

            /// <summary>
            /// Unrecognized arguments
            /// </summary>
            public IReadOnlyCollection<UnrecognizedArgument> Unrecognized { get; }
            public Properties(IReadOnlyCollection<RecognizedArgument> recognized, 
                IReadOnlyCollection<UnrecognizedArgument> unrecognized)
            {
                Recognized = recognized;
                Unrecognized = unrecognized;
            }
        }

        /// <summary>
        /// Map from <see cref="ParsedArguments"/> to <see cref="T"/>.
        /// </summary>
        public T Select<T>(Func<Method, T> method, Func<Merged, T> merged, Func<Properties, T> properties, Func<MethodMissingArguments,T> methodMissingArguments)
        {
            switch (this)
            {
                case Method pm: return method(pm);
                case Merged m: return merged(m);
                case Properties d: return properties(d);
                case MethodMissingArguments e: return methodMissingArguments(e);
                default:
                    throw new Exception("Unimplemented switch case");
            }
        }

    }
}

