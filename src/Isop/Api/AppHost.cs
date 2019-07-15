using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using Isop.Help;
using Isop.Localization;

namespace Isop.Api
{
    using CommandLine;
    using CommandLine.Lex;
    using CommandLine.Parse;
    using Domain;
    using Abstractions;
    /// <summary>
    /// AppHost contains the service provider and configuration needed to run a command line app
    /// </summary>
    public class AppHost
    {
        internal readonly Formatter Formatter;
        internal readonly Recognizes Recognizes;
        internal readonly IServiceProvider ServiceProvider;
        internal readonly ControllerRecognizer ControllerRecognizer;
        private readonly IOptions<Configuration> _options;
        private HelpController _helpController;
        private IOptions<Texts> _texts;

        internal HelpController HelpController =>
            _helpController??(_helpController = ServiceProvider.GetService<HelpController>() ?? 
                                                new HelpController(
                                                    _texts,
                                                    Recognizes,
                                                    _options,
                                                    ServiceProvider));

        /// <summary>
        /// 
        /// </summary>
        public AppHost(IOptions<Configuration> options,
            IServiceProvider serviceProvider,
            Recognizes recognizes,
            TypeConverter typeConverter, 
            Formatter formatter, 
            IOptions<Texts> texts)
        {
            Formatter = formatter;
            _options = options;
            Recognizes = recognizes;
            ServiceProvider = serviceProvider;
            ControllerRecognizer = new ControllerRecognizer(options,
                typeConverter);
            _texts = texts;
        }
        /// <summary>
        /// Parse command line arguments and return parsed arguments entity
        /// </summary>
        public ParsedExpression Parse(IEnumerable<string> arg) => Parse(arg.ToList());

        private ParsedExpression Parse(IReadOnlyCollection<string> arg)
        {
            var argumentParser = new ArgumentParser(Recognizes.Properties.Select(p=>p.ToArgument(_options.Value.CultureInfo)), AllowInferParameter, CultureInfo);
            var lexed = ArgumentLexer.Lex(arg).ToList();
            var parsedArguments = argumentParser.Parse(lexed, arg);
            if (Recognizes.Controllers.Any())
            {
                var recognizedController = Recognizes.Controllers
                    .FirstOrDefault(controller => ControllerRecognizer.Recognize(controller, arg));
                if (null != recognizedController)
                {
                    return new ParsedExpression(ControllerRecognizer.ParseArgumentsAndMerge(recognizedController, arg, parsedArguments), this);
                }
            }
            parsedArguments.AssertFailOnUnMatched();
            return new ParsedExpression(parsedArguments, this);
        }
        
        internal bool AllowInferParameter => !(_options.Value?.DisableAllowInferParameter ?? false);
        internal CultureInfo CultureInfo => _options.Value?.CultureInfo;
        /// <summary>
        /// Return help-text
        /// </summary>
        public String Help()
        {
            var output = new StringWriter(CultureInfo);
            Parse(new[] { Conventions.Help }).Invoke(output);
            return output.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        public ControllerExpression Controller(string controllerName)
        {
            return new ControllerExpression(controllerName, this);
        }

    }
}
