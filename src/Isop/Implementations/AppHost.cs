using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Isop.Implementations
{
    using Abstractions;
    using CommandLine;
    using CommandLine.Lex;
    using CommandLine.Parse;
    using CommandLine.Views;
    using Domain;
    using Help;
    using Localization;

    internal class AppHost : IAppHost
    {
        internal readonly Formatter Formatter;
        internal readonly Recognizes Recognizes;
        internal readonly IServiceProvider ServiceProvider;
        internal readonly ControllerRecognizer ControllerRecognizer;
        internal readonly IOptions<Conventions> Conventions;
        internal readonly IOptions<Configuration> Configuration;
        internal readonly TypeConverter TypeConverter;
        private HelpController _helpController;
        private readonly IOptions<Texts> _texts;
        

        internal HelpController HelpController =>
            _helpController??(_helpController = ServiceProvider.GetService<HelpController>() ?? 
                                                new HelpController(
                                                    _texts,
                                                    Recognizes,
                                                    Configuration,
                                                    ServiceProvider,
                                                    Conventions));

        /// <summary>
        /// 
        /// </summary>
        public AppHost(IOptions<Configuration> options,
            IServiceProvider serviceProvider,
            Recognizes recognizes,
            TypeConverter typeConverter, 
            Formatter formatter, 
            IOptions<Texts> texts,
            IOptions<Conventions> conventions)
        {
            Formatter = formatter;
            Configuration = options ?? Options.Create(new Configuration());
            Recognizes = recognizes;
            ServiceProvider = serviceProvider;
            Conventions = conventions?? Options.Create(new Conventions());
            TypeConverter = typeConverter;
            ControllerRecognizer = new ControllerRecognizer(options,
                typeConverter, Conventions, Recognizes);
            _texts = texts ?? Options.Create(new Texts());
        }


        /// <summary>
        /// Parse command line arguments and return parsed arguments entity
        /// </summary>
        public IParsedExpression Parse(IEnumerable<string> arg) => Parse(arg.ToList());

        private IParsedExpression Parse(IReadOnlyCollection<string> arg)
        {
            var argumentParser = new ArgumentParser(
                Recognizes.Properties.Select(p=>p.ToArgument(Configuration.Value.CultureInfo)).ToArray(),
                AllowInferParameter);
            var lexed = ArgumentLexer.Lex(arg).ToList();
            var parsedGlobalArgs = argumentParser.Parse(lexed, arg);
            if (!ControllerRecognizer.TryRecognize(arg, out var controllerAndMethodAndTokens))
                return new ParsedExpression(parsedGlobalArgs, this);
            
            var (controller, method) = controllerAndMethodAndTokens;
            return new ParsedExpression(parsedGlobalArgs.Merge(
                ControllerRecognizer.Parse(controller, method, arg)), this);
        }
        
        internal bool AllowInferParameter => !(Configuration.Value?.DisableAllowInferParameter ?? false);
        internal CultureInfo CultureInfo => Configuration.Value?.CultureInfo;
        /// <summary>
        /// Return help-text
        /// </summary>
        public String Help()
        {
            var output = new StringWriter(CultureInfo);
            Parse(new[] { Conventions.Value.Help }).Invoke(output);
            return output.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        public IControllerExpression Controller(string controllerName)
        {
            return new ControllerExpression(controllerName, this);
        }

    }
}
