using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace ConsoleHelpers
{
    delegate void ActionDelegate();
    public class Argument
    {
        private readonly Predicate<string> _recognizes;
        public string Longname { get; private set; }
        public Argument(string longname)
            : this(longname, null)
        {        }
        public Argument(string longname, Predicate<string> recognizes)
        {
            _recognizes = recognizes;
            Longname = longname;
        }

        public bool Recognizes(string argument)
        {
            return null != _recognizes ? _recognizes(argument) : DefaultRecognizer(argument);
        }

        public bool DefaultRecognizer(string argument)
        {
            if (argument.StartsWith("-" + Longname[0]))
                return true;
            if (argument.StartsWith("--" + Longname))
                return true;
            return false;
        }
    }

    public class ArgumentWithParameters
    {
        public string Value { get; private set; }
        public Argument Argument { get; private set; }
        public string Parameter { get; private set; }

        public ArgumentWithParameters(Argument argument, string parameter, string value = null)
        {
            Value = value;
            Argument = argument;
            Parameter = parameter;
        }
    }
    public class ParsedArguments
    {
        public IEnumerable<ArgumentWithParameters> InvokedArguments { get; set; }

    }
    public class ArgumentParser
    {
        public static ArgumentParserBuilder Build() { return new ArgumentParserBuilder(); }
        private readonly IEnumerable<Argument> _actions;

        public ArgumentParser(IEnumerable<Argument> actions)
        {
            _actions = actions;
        }

        public IEnumerable<ArgumentWithParameters> InvokedArguments { get; private set; }

        public ParsedArguments Parse(IEnumerable<string> arguments)
        {
            var argumentList = arguments.ToList();
            InvokedArguments = _actions.Select(act =>
                                                      new
                                                          {
                                                              arguments = argumentList.FindIndexAndValues(act.Recognizes),
                                                              action = act
                                                          })
                .Where(couple => couple.arguments.Any())
                .Select(couple => new ArgumentWithParameters(
                                      couple.action,
                                      couple.arguments.First().Value,
                                      argumentList.GetForIndexOrDefault(couple.arguments.First().Key + 1)));
            return new ParsedArguments { InvokedArguments = InvokedArguments };
        }
    }
}
