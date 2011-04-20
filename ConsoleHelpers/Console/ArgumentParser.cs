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
    public class ArgumentName
    {
        public ArgumentName(string value)
            : this(value, null, value)
        { }

        public ArgumentName(string value, string shortValue,string original)
            
        {
            Value = value;
            ShortValue = shortValue;
            this.original = original;
        }
        private string original;
        public string Value { get; private set; }
        public string ShortValue { get; private set; }
        private static Regex argPattern = new Regex(@"\&(.)");
        public static implicit operator ArgumentName(string value)
        {
            var match = argPattern.Match(value);
            if (match.Success)
                return new ArgumentName(value.Replace("&",""),match.Groups[1].Value, value);
            return new ArgumentName(value, null, value);
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
        public override bool Equals(object obj)
        {
            if (obj is ArgumentName)
            {
                var other = (ArgumentName)obj;
                return Value.Equals(other.Value) && ShortValue.Equals(other.ShortValue);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return this.Value.GetHashCode() ^ this.ShortValue.GetHashCode();
        }
        public override string ToString()
        {
            return original;
        }
    }
    /// <summary>
    /// class to enable extensions of the behavior of what is recognized as arguments.
    /// </summary>
    public class ArgumentRecognizer
    {
        private readonly Predicate<string> _recognizes;
        public ArgumentName ArgumentName { get; private set; }
        public Action<string> Action { get; set; }

        public ArgumentRecognizer(ArgumentName argumentName)
            : this(argumentName, null,null)
        { }

        public ArgumentRecognizer(ArgumentName argumentName, Predicate<string> recognizes)
            : this(argumentName, recognizes, null)
        { }

        public ArgumentRecognizer(ArgumentName argumentName, Predicate<string> recognizes,Action<string> action)
        {
            _recognizes = recognizes;
            ArgumentName = argumentName;
            Action = action;
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

        public IEnumerable<ArgumentRecognizer> Recognizers { get; set; }

        public string UnRecognizedArgumentsMessage()
        {
            return 
string.Format(@"Unrecognized arguments: 
{0}
Did you mean any of these arguments?
{1}", String.Join(",",UnRecognizedArguments.Select(arg=>arg.Value).ToArray()), 
    String.Join(",",Recognizers.Select(rec=>rec.ArgumentName.ToString()).ToArray()));
        }

        public void Invoke()
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
            var recognized = _recognizers.Select(act =>
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
                Recognizers = _recognizers.ToArray(),
                RecognizedArguments = invokedArguments,
                UnRecognizedArguments = unRecognizedArguments
            };
        }
    }
}
