using System;
using System.Linq;
using System.Collections.Generic;

namespace ConsoleHelpers
{
    /// <summary>
    /// Represents the long name of an argument. For instance file of the commandline argument --file. 
    /// Usually you might want it to recognize -f as well.
    /// </summary>
    public struct ArgumentLongname
    {
        public ArgumentLongname(string value)
            : this(value, null)
        {
        }

        public ArgumentLongname(string value, string shortValue)
            : this()
        {
            Value = value;
            ShortValue = shortValue;
        }
        public string Value { get; private set; }
        public string ShortValue { get; private set; }

        public static implicit operator ArgumentLongname(string value)
        {
            return new ArgumentLongname(value, null);
        }
        public bool Recognize(string argument)
        {
            return (!String.IsNullOrEmpty(ShortValue) ? argument.StartsWith("-" + ShortValue) : false)
                || argument.StartsWith("--" + Value);
        }
    }
    public class ArgumentRecognizer
    {
        private readonly Predicate<string> _recognizes;
        public ArgumentLongname ArgumentLongname { get; private set; }
        public ArgumentRecognizer(ArgumentLongname argumentLongname)
            : this(argumentLongname, null)
        { }

        public ArgumentRecognizer(ArgumentLongname argumentLongname, Predicate<string> recognizes)
        {
            _recognizes = recognizes;
            ArgumentLongname = argumentLongname;
        }

        public bool Recognizes(string argument)
        {
            return null != _recognizes ? _recognizes(argument) : ArgumentLongname.Recognize(argument);
        }
    }

    public class RecognizedArgument
    {
        public string Value { get; private set; }
        public ArgumentRecognizer ArgumentRecognizer { get; private set; }
        public string Argument { get; private set; }

        public RecognizedArgument(ArgumentRecognizer argumentRecognizer, string parameter, string value = null)
        {
            Value = value;
            ArgumentRecognizer = argumentRecognizer;
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
            var invokedArguments = recognized
                .Select(couple => new RecognizedArgument(
                                      couple.action,
                                      couple.arguments.First().Value,
                                      argumentList.GetForIndexOrDefault(couple.arguments.First().Key + 1)));
            var recognizedIndexes = recognized.SelectMany(couple => couple.arguments.Select(arg => arg.Key))
                .Distinct();


            var unRecognizedArguments = argumentList
                .Select((value, i) => new { i, value })
                .Where(indexAndValue => !recognizedIndexes.Contains(indexAndValue.i))
                .Select(v => new UnrecognizedValue(v.value));

            return new ParsedArguments { RecognizedArguments = invokedArguments, UnRecognizedArguments = unRecognizedArguments };
        }
    }
}
