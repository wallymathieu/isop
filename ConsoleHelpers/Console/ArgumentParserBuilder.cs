using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Helpers.Console
{
    public class ArgumentParserBuilder
    {
        private readonly IList<ArgumentWithOptions> _argumentRecognizers;
        private readonly IList<ClassAndMethodRecognizer> _classAndMethodRecognizers;
        private CultureInfo _cultureInfo;
        private TypeConverterFunc _typeConverter;
		private Func<Type,Object> _factory;
        private readonly HelpForClassAndMethod _helpForClassAndMethod;
        private readonly HelpForArgumentWithOptions _helpForArgumentWithOptions;
        private HelpController _helpController;

        public ArgumentParserBuilder()
        {
            _classAndMethodRecognizers = new List<ClassAndMethodRecognizer>();
            _argumentRecognizers = new List<ArgumentWithOptions>();
            _helpForClassAndMethod = new HelpForClassAndMethod(_classAndMethodRecognizers);
            _helpForArgumentWithOptions = new HelpForArgumentWithOptions(_argumentRecognizers);
            _helpController = new HelpController(_helpForArgumentWithOptions, _helpForClassAndMethod);
			Recognize(_helpController);
        }

        public ArgumentParserBuilder Parameter(ArgumentParameter argument, Action<string> action = null, bool required = false, string description = null)
        {
            _argumentRecognizers.Add(new ArgumentWithOptions(argument, action, required, description));
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
		
		public ArgumentParserBuilder SetFactory(Func<Type,Object> factory)
		{
			this._factory = factory;
			return this;
		}
		
        public ParsedArguments Parse(IEnumerable<string> arg)
        {
			var argumentParser = new ArgumentParser(_argumentRecognizers);
            
			var parsedArgs = argumentParser.Parse(arg);
			
            if (_classAndMethodRecognizers.Any())
            {
                var methodRecognizer = _classAndMethodRecognizers.FirstOrDefault(recognizer => recognizer.Recognize(arg));
                if (null != methodRecognizer)
                {
					var parsedMethod = methodRecognizer.Parse(arg);
					parsedMethod.Factory = this._factory;
                    return parsedArgs.Merge( parsedMethod);
                }
            }
            return parsedArgs;
        }

        public ArgumentParserBuilder Recognize(Type arg, CultureInfo cultureInfo = null, TypeConverterFunc typeConverter = null)
        {
            _classAndMethodRecognizers.Add(new ClassAndMethodRecognizer(null, arg, _cultureInfo ?? cultureInfo, _typeConverter ?? typeConverter));
            return this;
        }
		public ArgumentParserBuilder Recognize(Object arg, CultureInfo cultureInfo = null, TypeConverterFunc typeConverter = null)
        {
            _classAndMethodRecognizers.Add(new ClassAndMethodRecognizer(arg, arg.GetType(), _cultureInfo ?? cultureInfo, _typeConverter ?? typeConverter));
            return this;
        }
		
        public ArgumentParserBuilder Argument(Argument argument, Action<string> action = null, bool required = false, string description = null)
        {
            _argumentRecognizers.Add(new ArgumentWithOptions(argument, action, required, description));
            return this;
        }

        public String Help()
        {
			return this.Parse(new []{"Help"}).Invoke();// return _helpController.Index();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="theCommandsAre">default: "The commands are:"</param>
        /// <param name="helpCommandForMoreInformation">default: "Se 'COMMANDNAME' help command for more information"</param>
        /// <returns></returns>
        public ArgumentParserBuilder HelpTextCommandsAre(string theCommandsAre, string helpCommandForMoreInformation)
        {
            _helpForClassAndMethod.TheCommandsAre = theCommandsAre;
            _helpForClassAndMethod.HelpCommandForMoreInformation = helpCommandForMoreInformation;
            return this;
        }
        
        public ArgumentParserBuilder HelpTextArgumentsAre(string TheArgumentsAre)
        {
            _helpForArgumentWithOptions.TheArgumentsAre = TheArgumentsAre;
            return this;
        }

        public string HelpFor(string command)
        {
            return this.Parse(new []{"Help",command}).Invoke();//_helpController.Index(command);
        }
    }
   
}