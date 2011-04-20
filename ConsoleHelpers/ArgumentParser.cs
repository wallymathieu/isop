using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace ConsoleHelpers
{
    public class ArgumentRecognizer
    {
        private readonly Predicate<string> _recognizes;
        public string ArgumentLongname { get; private set; }
        public ArgumentRecognizer(string argumentLongname)
            : this(argumentLongname, null)
        {}

        public ArgumentRecognizer(string argumentLongname, Predicate<string> recognizes)
        {
            _recognizes = recognizes;
            ArgumentLongname = argumentLongname;
        }

        public bool Recognizes(string argument)
        {
            return null != _recognizes ? _recognizes(argument) : DefaultRecognizer(argument);
        }

        public bool DefaultRecognizer(string argument)
        {
            if (argument.StartsWith("-" + ArgumentLongname[0]))
                return true;
            if (argument.StartsWith("--" + ArgumentLongname))
                return true;
            return false;
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
                .Select((value, i) => new {i, value})
                .Where(indexAndValue =>  !recognizedIndexes.Contains(indexAndValue.i))
                .Select(v=>new UnrecognizedValue (v.value));
                
            return new ParsedArguments { RecognizedArguments = invokedArguments, UnRecognizedArguments = unRecognizedArguments };
        }
    }
}
