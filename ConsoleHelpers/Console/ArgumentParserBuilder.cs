using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Helpers.Console
{
    public class ArgumentParserBuilder
    {
        private IList<ArgumentWithOptions> _argumentRecognizers = new List<ArgumentWithOptions>();
        private IList<ClassAndMethodRecognizer> _classAndMethodRecognizers = new List<ClassAndMethodRecognizer>();
        private CultureInfo _cultureInfo;

        public ArgumentParserBuilder Recognize(Argument argument, bool required = false)
        {
            if (_classAndMethodRecognizers.Any()) throw new NotImplementedException("Will not recognize any non class recognizers.");
            _argumentRecognizers.Add(new ArgumentWithOptions(argument, null, required));
            return this;
        }

        public ArgumentParserBuilder Action(Argument argument, Action<string> action, bool required = false)
        {
            if (_classAndMethodRecognizers.Any()) throw new NotImplementedException("Will not recognize any non class recognizers.");
            _argumentRecognizers.Add(new ArgumentWithOptions(argument, action, required));
            return this;
        }
        /// <summary>
        /// Sets the cultureinfo for the following calls.
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public ArgumentParserBuilder SetCulture(CultureInfo cultureInfo)
        {
            _cultureInfo = cultureInfo; return this;
        }

        public ParsedArguments Parse(IEnumerable<string> arg)
        {
            var methodRecognizer = _classAndMethodRecognizers.FirstOrDefault(recognizer => recognizer.Recognize(arg));
            if (null != methodRecognizer)
            {
                _classAndMethodRecognizers = new List<ClassAndMethodRecognizer>();
                return methodRecognizer.Parse(arg);
            }
            var argumentParser = new ArgumentParser(_argumentRecognizers);
            _argumentRecognizers = new List<ArgumentWithOptions>();

            return argumentParser.Parse(arg);
        }
        
        public ArgumentParserBuilder Recognize(Type arg, CultureInfo cultureInfo = null)
        {
            if (_argumentRecognizers.Any()) throw new NotImplementedException("Will not recognize any non class recognizers.");
            _classAndMethodRecognizers.Add(new ClassAndMethodRecognizer(arg, _cultureInfo ?? cultureInfo));
            return this;
        }
    }
}