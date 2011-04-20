using System;
using System.Collections.Generic;

namespace ConsoleHelpers
{
    public class ArgumentParserBuilder
    {
        private readonly IList<ArgumentRecognizer> _actions = new List<ArgumentRecognizer>();
        public ArgumentParserBuilder Recognize(ArgumentName argumentName)
        {
            return Recognize(argumentName, null);
        }

        public ArgumentParserBuilder Recognize(ArgumentName argumentName, Predicate<string> recognizes)
        {
            _actions.Add(new ArgumentRecognizer(argumentName,recognizes));
            return this;
        }

        public ParsedArguments Parse(IEnumerable<string> arg)
        {
            var argumentParser = new ArgumentParser(_actions);
            return argumentParser.Parse(arg);
        }
    }
}