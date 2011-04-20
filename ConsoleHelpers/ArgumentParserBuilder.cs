using System;
using System.Collections.Generic;

namespace ConsoleHelpers
{
    public class ArgumentParserBuilder
    {
        private readonly IList<Argument> _actions = new List<Argument>();

        public ArgumentParserBuilder Argument(string longname)
        {
            return Argument(longname, null);
        }

        public ArgumentParserBuilder Argument(string longname, Predicate<string> recognizes)
        {
            _actions.Add(new Argument(longname,recognizes));
            return this;
        }

        public ParsedArguments Parse(IEnumerable<string> arg)
        {
            var argumentParser = new ArgumentParser(_actions);
            return argumentParser.Parse(arg);
        }
    }
}