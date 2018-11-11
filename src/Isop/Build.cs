using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace Isop
{
    using Infrastructure;
    using Help;
    using CommandLine;
    using CommandLine.Lex;
    using CommandLine.Parse;
    using CommandLine.Help;
    using Configurations;
    using Domain;
    using Api;

    /// <summary>
    /// represents a configuration build
    /// </summary>
    public class Build : IEnumerable<Type>
    {
        internal HelpForControllers HelpForControllers;
        private HelpForArgumentWithOptions _helpForArgumentWithOptions;
        private HelpController _helpController;

        private readonly Configuration _configuration = new Configuration();
        internal bool AllowInferParameter = true;

        public Build():this(new ServiceCollection())
        {
        }
        public Build(IServiceCollection collection)
        {
            this.Container = collection;
        }

        public virtual CultureInfo CultureInfo { get { return _configuration.CultureInfo; } set { _configuration.CultureInfo = value; } }

        public TypeConverterFunc TypeConverter
        {
            get { return _configuration.TypeConverter; }
            set { _configuration.TypeConverter = value; }
        }

        public bool RecognizeHelp
        {
            get { return _configuration.RecognizesHelp; }
            set { _configuration.RecognizesHelp = value; }
        }

        private readonly HelpXmlDocumentation _helpXmlDocumentation = new HelpXmlDocumentation();

        public IEnumerator<Type> GetEnumerator()
        {
            return _configuration.Recognizes.Select(c => c.Type).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(Type item)
        {
            Recognize(item);
        }

        public ICollection<Controller> Recognizes
        {
            get { return _configuration.Recognizes; }
        }

        public HelpXmlDocumentation HelpXmlDocumentation
        {
            get { return _helpXmlDocumentation; }
        }

        public Build Parameter(string argument, Action<string> action = null, bool required = false, string description = null)
        {
            _configuration.Properties.Add(new Property(argument, action, required, description, typeof(string)));
            return this;
        }

        public Build FormatObjectsAsTable()
        {
            _configuration.Formatter = new TableFormatter().Format;
            return this;
        }

        public Build SetFormatter(Formatter formatter)
        {
            _configuration.Formatter = formatter;
            return this;
        }

        public ParsedArguments Parse(IEnumerable<string> arg)
        {
            return Parse(arg.ToList());
        }

        public ParsedArguments Parse(List<string> arg)
        {
            var argumentParser = new ArgumentParser(GlobalParameters, AllowInferParameter, CultureInfo);
            var lexed = ArgumentLexer.Lex(arg).ToList();
            var parsedArguments = argumentParser.Parse(lexed, arg);
            if (ControllerRecognizers.Any())
            {
                var recognizers = ControllerRecognizers.Select(cr => cr.Value());
                var controllerRecognizer = recognizers.FirstOrDefault(recognizer => recognizer.Recognize(arg));
                if (null != controllerRecognizer)
                {
                    return controllerRecognizer.ParseArgumentsAndMerge(arg,
                        parsedArguments);
                }
            }
            parsedArguments.AssertFailOnUnMatched();
            return parsedArguments;
        }

        public Build Recognize(Type arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            _configuration.Recognizes.Add(new Controller(arg, ignoreGlobalUnMatchedParameters));
            Container.TryAddSingleton(arg);
            return this;
        }
        public Build Recognize(Object arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            var type = arg.GetType();
            _configuration.Recognizes.Add(new Controller(type, ignoreGlobalUnMatchedParameters));
            Container.AddSingleton(type, svc => arg);
            return this;
        }

        public Build DisallowInferParameter()
        {
            AllowInferParameter = false;
            return this;
        }

        public String Help()
        {
            var cout = new StringWriter(CultureInfo);
            Parse(new[] { Conventions.Help }).Invoke(cout);
            return cout.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        public Build HelpTextCommandsAre(Action<HelpTexts> action)
        {
            ShouldRecognizeHelp();
            HelpController();
            var helpForControllers = HelpForControllers;
            action(helpForControllers);
            return this;
        }

        public Build HelpTextArgumentsAre(string theArgumentsAre)
        {
            ShouldRecognizeHelp();
            HelpController();
            _helpForArgumentWithOptions.TheArgumentsAre = theArgumentsAre;
            return this;
        }

        public string HelpFor(string controller, string action = null)
        {
            var cout = new StringWriter(CultureInfo);
            Parse(new[] { Conventions.Help, controller, action }
                .Where(s => !string.IsNullOrEmpty(s))).Invoke(cout);
            return cout.ToString();
        }

        public Build ShouldRecognizeHelp()
        {
            _configuration.RecognizesHelp = true;
            return this;
        }

        public bool RecognizesHelp
        {
            get { return _configuration.RecognizesHelp; }
        }

        public IEnumerable<KeyValuePair<Type, Func<ControllerRecognizer>>> ControllerRecognizers
        {
            get
            {
                if (RecognizeHelp && !_configuration.Recognizes.Any(c => c.Type == typeof(HelpController)))
                {
                    Recognize(HelpController(), ignoreGlobalUnMatchedParameters: true);
                }

                return _configuration.Recognizes.Select(
                    c => new KeyValuePair<Type, Func<ControllerRecognizer>>(
                        c.Type,
                        () => new ControllerRecognizer(c, _configuration, Container, AllowInferParameter)));
            }
        }

        public IEnumerable<ArgumentWithOptions> GlobalParameters
        {
            get
            {
                return _configuration.Properties.Select(p =>
                    new ArgumentWithOptions(
                        argument: p.Name,
                        action: p.Action,
                        required: p.Required,
                        description: p.Description,
                        type: p.Type)).ToList();
            }
        }

        public IServiceCollection Container { get; }

        internal Build Configuration(Type t, object instance)
        {
            new ConfigureUsingInstance(_configuration, _helpXmlDocumentation).Configure(t, instance);
            return this;
        }

        protected internal HelpController HelpController()
        {
            if (_helpController == null && _configuration.RecognizesHelp)
            {
                HelpForControllers = new HelpForControllers(_configuration.Recognizes, Container,
                    HelpXmlDocumentation);
                _helpForArgumentWithOptions = new HelpForArgumentWithOptions(GlobalParameters);
                _helpController = new HelpController(_helpForArgumentWithOptions, HelpForControllers);
            }
            return _helpController;
        }

        public ControllerExpression Controller(string controllerName)
        {
            return new ControllerExpression(controllerName, this);
        }
    }
}
