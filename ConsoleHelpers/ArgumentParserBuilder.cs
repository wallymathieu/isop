using System;
using System.Collections.Generic;

namespace ConsoleHelpers
{
    public class ArgumentParserBuilder
    {
        private readonly IList<ArgumentRecognizer> _actions = new List<ArgumentRecognizer>();
        public ArgumentParserBuilder Recognize(ArgumentLongname argumentLongname)
        {
            return Recognize(argumentLongname, null);
        }

        public ArgumentParserBuilder Recognize(ArgumentLongname argumentLongname, Predicate<string> recognizes)
        {
            _actions.Add(new ArgumentRecognizer(argumentLongname,recognizes));
            return this;
        }

        public ParsedArguments Parse(IEnumerable<string> arg)
        {
            var argumentParser = new ArgumentParser(_actions);
            return argumentParser.Parse(arg);
        }
    }
}