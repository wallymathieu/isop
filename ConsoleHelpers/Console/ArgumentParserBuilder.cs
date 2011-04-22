using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Helpers.Console
{
    public class ArgumentParserBuilder
    {
        private IList<ArgumentRecognizer> _argumentRecognizers = new List<ArgumentRecognizer>();
        private IList<ClassAndMethodRecognizer> _classAndMethodRecognizers = new List<ClassAndMethodRecognizer>();
        private CultureInfo _cultureInfo;

        public ArgumentParserBuilder Recognize(Argument argument, Predicate<string> recognizes = null, bool required = false)
        {
            _argumentRecognizers.Add(new ArgumentRecognizer(argument, recognizes, null, required));
            return this;
        }

        public ArgumentParserBuilder Action(Argument argument, Action<string> action, Predicate<string> recognizes = null, bool required = false)
        {
            _argumentRecognizers.Add(new ArgumentRecognizer(argument, recognizes, action, required));
            return this;
        }
        public ArgumentParserBuilder SetCulture(CultureInfo cultureInfo)
        {
            _cultureInfo = cultureInfo; return this;
        }

        public ParsedMethod ParseMethod(IEnumerable<string> arg)
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
        
        public ArgumentParserBuilder Recognize(Type arg, CultureInfo cultureInfo = null)
        {
            _classAndMethodRecognizers.Add(new ClassAndMethodRecognizer(arg, _cultureInfo ?? cultureInfo));
            return this;
        }
    }
}