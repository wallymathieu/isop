using System;
using System.Collections.Generic;

namespace ConsoleHelpers
{
    public class ArgumentParserBuilder
    {
        private readonly IList<ArgumentRecognizer> _actions = new List<ArgumentRecognizer>();

        public ArgumentParserBuilder Recognize(string longname)
        {
            return Recognize(longname, null);
        }

        public ArgumentParserBuilder Recognize(string longname, Predicate<string> recognizes)
        {
            _actions.Add(new ArgumentRecognizer(longname,recognizes));
            return this;
        }

        public ParsedArguments Parse(IEnumerable<string> arg)
        {
            var argumentParser = new ArgumentParser(_actions);
            return argumentParser.Parse(arg);
        }
    }
}