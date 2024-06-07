using System.Collections.Generic;
using System;
namespace Isop.CommandLine.Parse
{
    public record ParsedArguments
    {
        public ParsedArguments Merge(ParsedArguments args)
        {
            return new Merged(this, args);
        }

        /// <summary>
        /// A combination of two parsed arguments instances
        /// </summary>
        public record Merged(ParsedArguments First, ParsedArguments Second) : ParsedArguments
        {
        }

        public record MethodMissingArguments(Type recognizedClass,
            Domain.Method recognizedAction,
            IReadOnlyCollection<string> missingParameters) : ParsedArguments
        {
            public Type RecognizedClass { get; } = recognizedClass;
            public Domain.Method RecognizedAction { get; } = recognizedAction;
            public IReadOnlyCollection<string> MissingParameters { get; } = missingParameters;
        }

        public record Method(Type RecognizedClass,
            Domain.Method RecognizedAction,
            IReadOnlyCollection<object> RecognizedActionParameters,
            IReadOnlyCollection<RecognizedArgument> Recognized) : ParsedArguments
        {
        }

        public record Properties(IReadOnlyCollection<RecognizedArgument> Recognized,
            IReadOnlyCollection<UnrecognizedArgument> Unrecognized) : ParsedArguments
        {
            /// <summary>
            /// Recognized arguments
            /// </summary>
            public IReadOnlyCollection<RecognizedArgument> Recognized { get; } = Recognized;

            /// <summary>
            /// Unrecognized arguments
            /// </summary>
            public IReadOnlyCollection<UnrecognizedArgument> Unrecognized { get; } = Unrecognized;
        }

        /// <summary>
        /// Map from <see cref="ParsedArguments"/> to <see typecref="T"/>.
        /// </summary>
        public T Select<T>(Func<Method, T> method, Func<Merged, T> merged, Func<Properties, T> properties, Func<MethodMissingArguments,T> methodMissingArguments)
        {
            return this switch
            {
                Method pm => method(pm),
                Merged m => merged(m),
                Properties d => properties(d),
                MethodMissingArguments e => methodMissingArguments(e),
                _ => throw new Exception("Unimplemented switch case"),
            };
        }

    }
}

