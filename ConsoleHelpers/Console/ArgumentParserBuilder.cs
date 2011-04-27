using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Helpers.Console
{
    public class ArgumentParserBuilder
    {
        private IList<ArgumentWithOptions> _argumentRecognizers = new List<ArgumentWithOptions>();
        private IList<ClassAndMethodRecognizer> _classAndMethodRecognizers = new List<ClassAndMethodRecognizer>();
        private CultureInfo _cultureInfo;
        private TypeConverterFunc _typeConverter;

        public ArgumentParserBuilder Parameter(ArgumentParameter argument, bool required = false)
        {
            if (_classAndMethodRecognizers.Any()) throw new NotImplementedException("Will not recognize any non class recognizers.");
            _argumentRecognizers.Add(new ArgumentWithOptions(argument, null, required));
            return this;
        }

        public ArgumentParserBuilder Parameter(ArgumentParameter argument, Action<string> action, bool required = false)
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
        public ArgumentParserBuilder SetTypeConverter(TypeConverterFunc typeconverter)
        {
            _typeConverter = typeconverter; return this;
        }

        public ParsedArguments Parse(IEnumerable<string> arg)
        {
            if (_classAndMethodRecognizers.Any())
            {
                var methodRecognizer = _classAndMethodRecognizers.FirstOrDefault(recognizer => recognizer.Recognize(arg));
                if (null != methodRecognizer)
                {
                    _classAndMethodRecognizers = new List<ClassAndMethodRecognizer>();
                    return methodRecognizer.Parse(arg);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("No class or method found.");
                }
            }
            var argumentParser = new ArgumentParser(_argumentRecognizers);
            _argumentRecognizers = new List<ArgumentWithOptions>();

            return argumentParser.Parse(arg);
        }
        
        public ArgumentParserBuilder Recognize(Type arg, CultureInfo cultureInfo = null, TypeConverterFunc typeConverter=null)
        {
            if (_argumentRecognizers.Any()) throw new NotImplementedException("Will not recognize any non class recognizers.");
            _classAndMethodRecognizers.Add(new ClassAndMethodRecognizer(arg, _cultureInfo ?? cultureInfo, _typeConverter ?? typeConverter));
            return this;
        }

        public ArgumentParserBuilder Argument(Argument argument,Action<string> action=null, bool required=false)
        {
            _argumentRecognizers.Add(new ArgumentWithOptions(argument,action,required));
            return this;
        }
    }
}