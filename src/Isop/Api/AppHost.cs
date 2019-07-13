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
    using Infrastructure;
    /// <summary>
    /// AppHost contains the service provider and configuration needed to run a command line app
    /// </summary>
    public class AppHost
    {
        internal readonly RecognizesConfiguration RecognizesConfiguration;
        internal readonly IServiceProvider ServiceProvider;
        internal readonly ControllerRecognizer ControllerRecognizer;
        private readonly Configuration _options;

        /// <summary>
        /// 
        /// </summary>
        public AppHost(IOptions<Configuration> options,
            IServiceProvider serviceProvider,
            RecognizesConfiguration recognizesConfiguration)
        {
            _options = options?.Value;
            RecognizesConfiguration = recognizesConfiguration;
            ServiceProvider = serviceProvider;
            ControllerRecognizer = new ControllerRecognizer(options, serviceProvider,
                serviceProvider.GetRequiredService<TypeConverterFunc>(),
                serviceProvider.GetRequiredService<Formatter>());
        }
        /// <summary>
        /// Parse command line arguments and return parsed arguments entity
        /// </summary>
        public ParsedArguments Parse(IEnumerable<string> arg)
        {
            return Parse(arg.ToList());
        }

        private ParsedArguments Parse(List<string> arg)
        {
            var argumentParser = new ArgumentParser(GlobalParameters, AllowInferParameter, CultureInfo);
            var lexed = ArgumentLexer.Lex(arg).ToList();
            var parsedArguments = argumentParser.Parse(lexed, arg);
            if (RecognizesConfiguration.Recognizes.Any())
            {
                var recognizedController = RecognizesConfiguration.Recognizes
                    .FirstOrDefault(controller => ControllerRecognizer.Recognize(controller, arg));
                if (null != recognizedController)
                {
                    return ControllerRecognizer.ParseArgumentsAndMerge(recognizedController, arg, parsedArguments);
                }
            }
            parsedArguments.AssertFailOnUnMatched();
            return parsedArguments;
        }
        
        internal bool AllowInferParameter => !(_options?.DisableAllowInferParameter ?? false);
        internal CultureInfo CultureInfo => _options?.CultureInfo;
        internal IEnumerable<ArgumentWithOptions> GlobalParameters => new GlobalArguments(RecognizesConfiguration).GlobalParameters;
        /// <summary>
        /// Return help-text
        /// </summary>
        public String Help()
        {
            var cout = new StringWriter(CultureInfo);
            Parse(new[] { Conventions.Help }).Invoke(cout);
            return cout.ToString();
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
