using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Helpers.Console
{
    /// <summary>
    /// Represents the long name of an argument. For instance "file" of the commandline argument --file. 
    /// Usually you might want it to recognize -f as well.
    /// </summary>
    public struct ArgumentName
    {
        public ArgumentName(string value)
            : this(value, null)
        { }

        public ArgumentName(string value, string shortValue)
            : this()
        {
            Value = value;
            ShortValue = shortValue;
        }
        public string Value { get; private set; }
        public string ShortValue { get; private set; }
        private static Regex argPattern = new Regex(@"\&(.)");
        public static implicit operator ArgumentName(string value)
        {
            var match = argPattern.Match(value);
            if (match.Success)
                return new ArgumentName(value.Replace("&",""),match.Groups[1].Value);
            return new ArgumentName(value, null);
        }
        /// <summary>
        /// Default recognizer of argument names. Will try to match "--name" and "-n" if shortvalue is supplied
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public bool Recognize(string argument)
        {
            return (!String.IsNullOrEmpty(ShortValue) ? argument.StartsWith("-" + ShortValue) : false)
                || argument.StartsWith("--" + Value);
        }
    }
    /// <summary>
    /// class to enable extensions of the behavior of what is recognized as arguments.
    /// </summary>
    public class ArgumentRecognizer
    {
        private readonly Predicate<string> _recognizes;
        public ArgumentName ArgumentName { get; private set; }
        public ArgumentRecognizer(ArgumentName argumentName)
            : this(argumentName, null)
        { }

        public ArgumentRecognizer(ArgumentName argumentName, Predicate<string> recognizes)
        {
            _recognizes = recognizes;
            ArgumentName = argumentName;
        }

        public bool Recognizes(string argument)
        {
            return null != _recognizes ? _recognizes(argument) : ArgumentName.Recognize(argument);
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
    public class UnrecognizedValue
    {
        public UnrecognizedValue(string value)
        {
            Value = value;
        }
        public string Value { get; private set; }
    }
    public class ParsedArguments
    {
        public IEnumerable<RecognizedArgument> RecognizedArguments { get; set; }

        public IEnumerable<UnrecognizedValue> UnRecognizedArguments { get; set; }

        public MethodInfo RecognizedAction { get; set; }

        public IEnumerable<object> RecognizedActionParameters { get; set; }
    }
    public class ArgumentParser
    {
        public static ArgumentParserBuilder Build() { return new ArgumentParserBuilder(); }
        private readonly IEnumerable<ArgumentRecognizer> _actions;

        public ArgumentParser(IEnumerable<ArgumentRecognizer> actions)
        {
            _actions = actions;
        }

        public ParsedArguments Parse(IEnumerable<string> arguments)
        {
            var argumentList = arguments.ToList();
            var recognized = _actions.Select(act =>
                                             new
                                                 {
                                                     arguments = argumentList.FindIndexAndValues(act.Recognizes),
                                                     action = act
                                                 })
                .Where(couple => couple.arguments.Any());
            var recognizedIndexes = recognized.SelectMany(couple => couple.arguments.Select(arg => arg.Key))
              .ToList();
            var invokedArguments = recognized
                .Select(couple =>
                            {
                                var value = string.Empty;
                                var index=couple.arguments.First().Key + 1;
                                if (index < argumentList.Count && index >= 0)
                                {
                                    value = argumentList[index];
                                    recognizedIndexes.Add(index);
                                }
                                return new RecognizedArgument(
                                    couple.action,
                                    couple.arguments.First().Value,
                                    value);
                            })
                            .ToList();//In order to execute the above select
      

            var unRecognizedArguments = argumentList
                .Select((value, i) => new { i, value })
                .Where(indexAndValue => !recognizedIndexes.Contains(indexAndValue.i))
                .Select(v => new UnrecognizedValue(v.value));

            return new ParsedArguments
            {
                RecognizedArguments = invokedArguments,
                UnRecognizedArguments = unRecognizedArguments
            };
        }
    }
}
