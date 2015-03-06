using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

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
    /// <summary>
    /// represents a configuration build
    /// </summary>
    public class Build : ICollection<Type>, IDisposable
    {
        private HelpForControllers _helpForControllers;
        private HelpForArgumentWithOptions _helpForArgumentWithOptions;
        private HelpController _helpController;
        private readonly TypeContainer _container;
        private readonly Configuration _configuration = new Configuration();
        public virtual CultureInfo CultureInfo { get { return _configuration.CultureInfo; } set { _configuration.CultureInfo = value; } }
        public Func<Type, object> Factory
        {
            get
            {
                return _configuration.Factory;
            }
            set
            {
                _configuration.Factory = value;
            }
        }

        public TypeConverterFunc TypeConverter
        {
            get { return _configuration.TypeConverter; }
            set { _configuration.TypeConverter = value; }
        }

        public bool RecognizeHelp
        {
            get { return _configuration.RecognizesHelp; }
            set
            {
                _configuration.RecognizesHelp = value;
            }
        }

        readonly HelpXmlDocumentation _HelpXmlDocumentation = new HelpXmlDocumentation();

        public IEnumerator<Type> GetEnumerator()
        {
            return _configuration.Recognizes.Select(c => c.Type).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Type item)
        {
            Recognize(item);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Type item)
        {
            return _configuration.Recognizes.Any(kv => kv.Type == item);
        }

        public void CopyTo(Type[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Type item)
        {
            var withType = _configuration.Recognizes.Where(c => c.Type == item).ToArray();
            if (!withType.Any())
            {
                return false;
            }

            foreach (var element in withType)
            {
                _configuration.Recognizes.Remove(element);
            }
            return true;
        }

        public int Count
        {
            get { return _configuration.Recognizes.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public ICollection<Controller> Recognizes
        {
            get { return _configuration.Recognizes; }
        }

        public HelpXmlDocumentation HelpXmlDocumentation
        {
            get { return _HelpXmlDocumentation; }
        }

        public Build()
        {
            _container = new TypeContainer(_configuration);
        }

        public Build Parameter(string argument, Action<string> action = null, bool required = false, string description = null)
        {
            _configuration.Properties.Add(new Property(argument, action, required, description, typeof(string)));
            return this;
        }
        /// <summary>
        /// Sets the cultureinfo for the following calls.
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public Build SetCulture(CultureInfo cultureInfo)
        {
            this.CultureInfo = cultureInfo; return this;
        }

        public Build SetTypeConverter(TypeConverterFunc typeconverter)
        {
            TypeConverter = typeconverter; return this;
        }

        public Build SetFactory(Func<Type, Object> factory)
        {
            Factory = factory;
            return this;
        }

        public Build FormatObjectsAsTable()
        {
            _configuration.Formatter = new TableFormatter();
            return this;
        }

        public ParsedArguments Parse(IEnumerable<string> arg)
        {
            return Parse(arg.ToList());
        }

        public ParsedArguments Parse(List<string> arg)
        {
            var argumentParser = new ArgumentParser(GlobalParameters, _allowInferParameter, CultureInfo);
            var lexed = ArgumentLexer.Lex(arg).ToList();
            var parsedArguments = argumentParser.Parse(lexed, arg);
            if (ControllerRecognizers.Any())
            {
                var recognizers = ControllerRecognizers.Select(cr => cr.Value());
                var controllerRecognizer = recognizers.FirstOrDefault(recognizer => recognizer.Recognize(arg));
                if (null != controllerRecognizer)
                {
                    return controllerRecognizer.ParseArgumentsAndMerge(arg,
                        parsedArguments,
                        parsedMethod => parsedMethod.Configuration = _configuration);
                }
            }
            parsedArguments.AssertFailOnUnMatched();
            return parsedArguments;
        }

        public Build Recognize<T>(bool ignoreGlobalUnMatchedParameters = false)
        {
            return Recognize(typeof(T), ignoreGlobalUnMatchedParameters);
        }

        public Build Recognize(Type arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            _configuration.Recognizes.Add(new Controller(arg, ignoreGlobalUnMatchedParameters));
            return this;
        }
        public Build Recognize(Object arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            var type = arg.GetType();
            _configuration.Recognizes.Add(new Controller(type, ignoreGlobalUnMatchedParameters));
            _container.Instances.Add(type, arg);
            return this;
        }

        public Build DisallowInferParameter()
        {
            _allowInferParameter = false;
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
            var helpForControllers = _helpForControllers;
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
                        () => new ControllerRecognizer(c, _configuration, _allowInferParameter)));
            }
        }

        public IEnumerable<ArgumentWithOptions> GlobalParameters
        {
            get
            {
                return _configuration.Properties.Select(p => new ArgumentWithOptions(p.Name, p.Action, p.Required, p.Description, p.Type)).ToList();
            }
        }

        public Func<Type, object> GetFactory()
        {
            return _container.CreateInstance;
        }
        private List<IDisposable> disposables = new List<IDisposable>();
        private bool _allowInferParameter = true;

        public void Dispose()
        {
            foreach (var item in disposables)
            {
                item.Dispose();
            }
            disposables.Clear();
        }

        public Build Configuration<T>(T instance)
        {
            return Configuration(typeof(T), instance);
        }
        internal Build Configuration(Type t, object instance)
        {
            new ConfigureUsingInstance(_configuration, _HelpXmlDocumentation).Configure(t, instance);

            if (instance is IDisposable)
                disposables.Add((IDisposable)instance);

            return this;
        }

        public Build Configuration(Type type)
        {
            return Configuration(type, Activator.CreateInstance(type));
        }

        public Build ConfigurationFromAssemblyPath()
        {
            return ConfigurationFrom(ExecutionAssembly.Path());
        }
        /// <summary>
        /// Will load all the assemblies in the path in order to scan them.
        /// </summary>
        public Build ConfigurationFrom(string path)
        {
            var loadAssemblies = new LoadAssemblies();
            var assemblies = loadAssemblies.LoadFrom(path);
            foreach (var assembly in assemblies)
            {
                ConfigurationFrom(assembly);
            }
            return this;
        }

        public Build ConfigurationFrom(Assembly assembly)
        {
            var autoConfiguration = new AssemblyScanner(assembly);
            var isopconfigurations = autoConfiguration.IsopConfigurations()
                .ToArray();
            foreach (var config in isopconfigurations)
            {
                Configuration(config);
            }

            if (!isopconfigurations.Any())
            {
                foreach (var item in autoConfiguration.LooksLikeControllers())
                {
                    this.Recognize(item);
                }
            }
            return this;
        }

        protected internal HelpController HelpController()
        {
            if (_helpController == null && _configuration.RecognizesHelp)
            {
                _helpForControllers = new HelpForControllers(_configuration.Recognizes, _container,
                    HelpXmlDocumentation);
                _helpForArgumentWithOptions = new HelpForArgumentWithOptions(GlobalParameters);
                _helpController = new HelpController(_helpForArgumentWithOptions, _helpForControllers);
            }
            return _helpController;
        }

        public class ActionControllerExpression
        {
            private string controllerName;
            private string actionName;
            private Build build;

            public ActionControllerExpression(string controllerName, string actionName, Build build)
            {
                this.controllerName = controllerName;
                this.actionName = actionName;
                this.build = build;
            }
            public ParsedArguments Parameters(Dictionary<string, string> arg)
            {
                var argumentParser = new ArgumentParser(build.GlobalParameters, build._allowInferParameter, build.CultureInfo);
                var parsedArguments = argumentParser.Parse(arg);
                if (build.ControllerRecognizers.Any())
                {
                    var recognizers = build.ControllerRecognizers.Select(cr => cr.Value());
                    var controllerRecognizer = recognizers.FirstOrDefault(recognizer => recognizer.Recognize(controllerName, actionName));
                    if (null != controllerRecognizer)
                    {
                        return controllerRecognizer.ParseArgumentsAndMerge(actionName, arg,
                            parsedArguments,
                            parsedMethod => parsedMethod.Configuration = build._configuration);
                    }
                }
                parsedArguments.AssertFailOnUnMatched();
                return parsedArguments;
            }
            /// <summary>
            /// 
            /// </summary>
            public string Help()
            {
                this.build.HelpController();
                if (this.build._helpForControllers != null)
                {
                    var controller = this.build.Recognizes.Single(c => c.Recognize(controllerName, actionName));
                    var method = controller.GetMethod(actionName);
                    return (this.build._helpForControllers.Description(controller, method) ?? String.Empty).Trim();
                }
                return null;
            }
        }

        public class ControllerExpression
        {
            private string controllerName;
            private Build build;

            public ControllerExpression(string controllerName, Build build)
            {
                this.controllerName = controllerName;
                this.build = build;
            }
            public ActionControllerExpression Action(string actionName)
            {
                return new ActionControllerExpression(controllerName, actionName, build);
            }
        }

        public ControllerExpression Controller(string controllerName)
        {
            return new ControllerExpression(controllerName, this);
        }
    }
}