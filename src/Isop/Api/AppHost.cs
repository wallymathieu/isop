using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

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
        internal readonly Recognizes Recognizes;
        internal readonly IServiceProvider ServiceProvider;
        internal readonly ControllerRecognizer ControllerRecognizer;
        private readonly Configuration _options;

        /// <summary>
        /// 
        /// </summary>
        public AppHost(IOptions<Configuration> options,
            IServiceProvider serviceProvider,
            Recognizes recognizes)
        {
            _options = options?.Value;
            Recognizes = recognizes;
            ServiceProvider = serviceProvider;
            ControllerRecognizer = new ControllerRecognizer(options,
                serviceProvider.GetRequiredService<TypeConverter>());
        }
        /// <summary>
        /// Parse command line arguments and return parsed arguments entity
        /// </summary>
        public ParsedExpression Parse(IEnumerable<string> arg) => Parse(arg.ToList());

        private ParsedExpression Parse(IReadOnlyCollection<string> arg)
        {
            var argumentParser = new ArgumentParser(Recognizes.Properties.Select(p=>p.ToArgument(_options.CultureInfo)), AllowInferParameter, CultureInfo);
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
        
        internal bool AllowInferParameter => !(_options?.DisableAllowInferParameter ?? false);
        internal CultureInfo CultureInfo => _options?.CultureInfo;
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
