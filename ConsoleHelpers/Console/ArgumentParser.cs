using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Helpers.Console
{
    public class MissingArgumentException : Exception
    {
        public List<string> Arguments;
        public MissingArgumentException() { }

        public MissingArgumentException(string message) : base(message) { }

        public MissingArgumentException(string message, Exception inner) : base(message, inner) { }
    }
    /// <summary>
    /// Represents the argument. For instance "file" of the commandline argument --file. 
    /// Usually you might want it to recognize -f as well. For instance using Argument.Parse("&file"), or the implicit 
    /// string cast operator.
    /// </summary>
    public class Argument
    {
        public Argument(string value)
            : this(value, null, value)
        { }

        public Argument(string value, string shortValue, string original)
        {
            Value = value;
            ShortValue = shortValue;
            _original = original;
        }
        private readonly string _original;
        public string Value { get; private set; }
        public string ShortValue { get; private set; }
        private static readonly Regex ArgPattern = new Regex(@"\&(.)");
        public static implicit operator Argument(string value)
        {
            return Parse(value);
        }

        public static Argument Parse(string value)
        {
            var match = ArgPattern.Match(value);
            if (match.Success)
                return new Argument(value.Replace("&", ""), match.Groups[1].Value, value);
            return new Argument(value, null, value);
        }

        /// <summary>
        /// Default recognizer of argument names. Will try to match "--name" and "-n" if shortvalue is supplied
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public bool Recognize(string argument)
        {
            return (!String.IsNullOrEmpty(ShortValue) ? argument.StartsWith("-" + ShortValue, StringComparison.OrdinalIgnoreCase) : false)
                || argument.StartsWith("--" + Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (obj is Argument)
            {
                var other = (Argument)obj;
                return Value.Equals(other.Value) && ShortValue.Equals(other.ShortValue);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode() ^ ShortValue.GetHashCode();
        }
        public override string ToString()
        {
            return _original;
        }
    }
    /// <summary>
    /// class to enable extensions of the behavior of what is recognized as arguments.
    /// </summary>
    public class ArgumentRecognizer
    {
        private readonly Predicate<string> _recognizes;
        public Argument Argument { get; private set; }
        public Action<string> Action { get; set; }
        public bool Required { get; set; }

        public ArgumentRecognizer(Argument argument, Predicate<string> recognizes = null, Action<string> action = null, bool required = false)
        {
            _recognizes = recognizes;
            Argument = argument;
            Action = action;
            Required = required;
        }

        public bool Recognizes(string argument)
        {
            return null != _recognizes ? _recognizes(argument) : Argument.Recognize(argument);
        }
    }

    public class RecognizedArgument
    {
        /// <summary>
        /// the matched value if any, for instance the "value" of the expression "--argument value"
        /// </summary>
        public string Value { get; private set; }
        public ArgumentRecognizer Recognizer { get; private set; }
        /// <summary>
        /// the "argument" of the expression "--argument"
        /// </summary>
        public string Argument { get; private set; }

        public RecognizedArgument(ArgumentRecognizer argumentRecognizer, string parameter, string value = null)
        {
            Value = value;
            Recognizer = argumentRecognizer;
            Argument = parameter;
        }
    }
    public class ParsedArguments
    {
        public ParsedArguments()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parsedArguments"></param>
        public ParsedArguments(ParsedArguments parsedArguments)
        {
            this.RecognizedArguments = parsedArguments.RecognizedArguments;
            this.Recognizers = parsedArguments.Recognizers;
            this.UnRecognizedArguments = parsedArguments.UnRecognizedArguments;
        }
        public IEnumerable<RecognizedArgument> RecognizedArguments { get; set; }

        public IEnumerable<string> UnRecognizedArguments { get; set; }

        public IEnumerable<ArgumentRecognizer> Recognizers { get; set; }

        public virtual void Invoke()
        {
            foreach (var argument in RecognizedArguments.Where(argument => null != argument.Recognizer.Action))
            {
                argument.Recognizer.Action(argument.Value);
            }
        }
    }
    public class ArgumentParser
    {
        public static ArgumentParserBuilder Build() { return new ArgumentParserBuilder(); }
        private readonly IEnumerable<ArgumentRecognizer> _recognizers;

        public ArgumentParser(IEnumerable<ArgumentRecognizer> recognizers)
        {
            _recognizers = recognizers;
        }

        public ParsedArguments Parse(IEnumerable<string> arguments)
        {
            var argumentList = arguments.ToList();
            var recognizedArguments = _recognizers.Select(act =>
                                             new
                                                 {
                                                     Arguments = argumentList.FindIndexAndValues(act.Recognizes),
                                                     Recognizer = act
                                                 })
                .Where(couple => couple.Arguments.Any());
            var recognizedIndexes = recognizedArguments
              .SelectMany(recognizedArgument => recognizedArgument.Arguments.Select(arg => arg.Key))
              .ToList();
            var invokedArguments = recognizedArguments
                .Select(couple =>
                            {
                                var value = string.Empty;
                                var index = couple.Arguments.First().Key + 1;
                                if (index < argumentList.Count && index >= 0)
                                {
                                    value = argumentList[index];
                                    recognizedIndexes.Add(index);
                                }
                                return new RecognizedArgument(
                                    couple.Recognizer,
                                    couple.Arguments.First().Value,
                                    value);
                            })
                            .ToList();//In order to execute the above select


            var unRecognizedArguments = argumentList
                .Select((value, i) => new { i, value })
                .Where(indexAndValue => !recognizedIndexes.Contains(indexAndValue.i))
                .Select(v => v.value);

            var unMatchedRequiredArguments = _recognizers.Where(recognizer => recognizer.Required)
                .Where(recognizer => !recognizedArguments.Any(recogn => recogn.Recognizer.Equals(recognizer)));
            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments") { Arguments = unMatchedRequiredArguments.Select(unmatched => unmatched.Argument.ToString()).ToList() };
            }
            return new ParsedArguments
            {
                Recognizers = _recognizers.ToArray(),
                RecognizedArguments = invokedArguments,
                UnRecognizedArguments = unRecognizedArguments
            };
        }
    }
}
