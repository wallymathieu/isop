using System;
using System.Collections.Generic;
using System.Linq;

namespace Helpers.Console
{
    public class ArgumentParserBuilder
    {
        private IList<ArgumentRecognizer> _argumentRecognizers = new List<ArgumentRecognizer>();
        private IList<ClassAndMethodRecognizer> _classAndMethodRecognizers = new List<ClassAndMethodRecognizer>();
        
        public ArgumentParserBuilder Recognize(ArgumentName argumentName)
        {
            return Recognize(argumentName, null);
        }

        public ArgumentParserBuilder Recognize(ArgumentName argumentName, Predicate<string> recognizes)
        {
            _argumentRecognizers.Add(new ArgumentRecognizer(argumentName,recognizes));
            return this;
        }

        public ArgumentParserBuilder RecognizeAction(ArgumentName argumentName, Action<string> action)
        {
            return RecognizeAction(argumentName, action, null);
        }
        public ArgumentParserBuilder RecognizeAction(ArgumentName argumentName, Action<string> action, Predicate<string> recognizes)
        {
            _argumentRecognizers.Add(new ArgumentRecognizer(argumentName, recognizes, action));
            return this;
        }
        public ClassAndMethodRecognizer.ParsedMethod ParseMethod(IEnumerable<string> arg)
        {
            var methodRecognizer = _classAndMethodRecognizers.FirstOrDefault(recognizer => recognizer.Recognize(arg));
            if (null != methodRecognizer)
            {
                _classAndMethodRecognizers = new List<ClassAndMethodRecognizer>();
                return methodRecognizer.Parse(arg);
            }
            throw new Exception("Missing matching method");
        }

        public ParsedArguments Parse(IEnumerable<string> arg)
        {
            var argumentParser = new ArgumentParser(_argumentRecognizers);
            _argumentRecognizers = new List<ArgumentRecognizer>();

            return argumentParser.Parse(arg);
        }

        public ArgumentParserBuilder Recognize(Type arg)
        {
            _classAndMethodRecognizers.Add(new ClassAndMethodRecognizer(arg));
            return this;
        }
    }
}