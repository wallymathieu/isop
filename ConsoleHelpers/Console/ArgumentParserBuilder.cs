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
		private Func<Type,Object> _factory;
        private HelpForClassAndMethod _helpForClassAndMethod = new HelpForClassAndMethod();
        private HelpForArgumentWithOptions _helpForArgumentWithOptions = new HelpForArgumentWithOptions();
        //public ArgumentParserBuilder Parameter(ArgumentParameter argument, bool required = false, string description = null)
        //{
        //    if (_classAndMethodRecognizers.Any()) throw new NotImplementedException("Will not recognize any non class recognizers.");
        //    _argumentRecognizers.Add(new ArgumentWithOptions(argument, null, required, description));
        //    return this;
        //}

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
            _argumentRecognizers = new List<ArgumentWithOptions>();
			var parsedArgs = argumentParser.Parse(arg);
			
            if (_classAndMethodRecognizers.Any())
            {
                var methodRecognizer = _classAndMethodRecognizers.FirstOrDefault(recognizer => recognizer.Recognize(arg));
                if (null != methodRecognizer)
                {
                    _classAndMethodRecognizers = new List<ClassAndMethodRecognizer>();
					var parsedMethod = methodRecognizer.Parse(arg);
					parsedMethod.Factory = this._factory;
                    return parsedArgs.Merge( parsedMethod);
                }
                else
                {
                    throw new NoClassOrMethodFoundException("No class or method found.");
                }
            }
            return parsedArgs;
        }

        public ArgumentParserBuilder Recognize(Type arg, CultureInfo cultureInfo = null, TypeConverterFunc typeConverter = null)
        {
            _classAndMethodRecognizers.Add(new ClassAndMethodRecognizer(arg, _cultureInfo ?? cultureInfo, _typeConverter ?? typeConverter));
            return this;
        }

        public ArgumentParserBuilder Argument(Argument argument, Action<string> action = null, bool required = false, string description = null)
        {
            _argumentRecognizers.Add(new ArgumentWithOptions(argument, action, required, description));
            return this;
        }

        public String Help()
        {
            if (_classAndMethodRecognizers.Any())
            {
                return _helpForClassAndMethod.Help(_classAndMethodRecognizers);
            }
            else
            {
                return _helpForArgumentWithOptions.Help(_argumentRecognizers);
            }
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
            return _helpForClassAndMethod.Help(_classAndMethodRecognizers, command);
        }
    }
    public class HelpForArgumentWithOptions
    {
        public string TheArgumentsAre { get; set; }

        public HelpForArgumentWithOptions()
        {
            TheArgumentsAre = "The arguments are:";
        }
        public string Help(IEnumerable<ArgumentWithOptions> argumentWithOptionses)
        {
            return TheArgumentsAre + Environment.NewLine +
                   String.Join(Environment.NewLine,
                               argumentWithOptionses.Select(ar => "  " + ar.Help()).ToArray());
        }
    }
    public class HelpForClassAndMethod
    {
        public string TheCommandsAre { get; set; }
        public string HelpCommandForMoreInformation { get; set; }
        
        public string HelpSubCommandForMoreInformation { get; set; }

        public HelpForClassAndMethod()
        {
            
            HelpSubCommandForMoreInformation = "Se 'COMMANDNAME' help <command> <subcommand> for more information";

            HelpCommandForMoreInformation = "Se 'COMMANDNAME' help <command> for more information";
            TheCommandsAre = "The commands are:";
        }

        public string Help(IEnumerable<ClassAndMethodRecognizer> classAndMethodRecognizers)
        {
            return TheCommandsAre + Environment.NewLine +
                   String.Join(Environment.NewLine,
                               classAndMethodRecognizers.Select(cmr => "  " + cmr.Help(true)).ToArray())
                   + Environment.NewLine
                   + Environment.NewLine
                   + HelpCommandForMoreInformation;
        }

        public string Help(IEnumerable<ClassAndMethodRecognizer> classAndMethodRecognizers, string command)
        {
            throw new NotImplementedException();
        }
    }
}