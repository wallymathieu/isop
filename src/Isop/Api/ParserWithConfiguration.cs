using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Isop.Api
{
    using Help;
    using CommandLine;
    using CommandLine.Lex;
    using CommandLine.Parse;
    using Domain;
    using Microsoft.Extensions.Options;
    using Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using System.Globalization;

    public class ParserWithConfiguration
    {
        internal readonly RecognizesConfiguration RecognizesConfiguration;
        internal readonly ControllerRecognizer ControllerRecognizer;
        private readonly Configuration _options;

        public ParserWithConfiguration(IOptions<Configuration> options,
            IServiceProvider serviceProvider,
            RecognizesConfiguration recognizesConfiguration)
        {
            _options = options.Value;
            RecognizesConfiguration = recognizesConfiguration;
            ControllerRecognizer = new ControllerRecognizer(options, serviceProvider,
                serviceProvider.GetRequiredService<TypeConverterFunc>(),
                serviceProvider.GetRequiredService<Formatter>());
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
        public string HelpFor(string controller, string action = null)
        {
            var cout = new StringWriter(CultureInfo);
            Parse(new[] { Conventions.Help, controller, action }
                .Where(s => !string.IsNullOrEmpty(s))).Invoke(cout);
            return cout.ToString();
        }
        public bool AllowInferParameter{ get =>!_options.DisableAllowInferParameter; }
        public CultureInfo CultureInfo { get => _options.CultureInfo; }
        public IEnumerable<ArgumentWithOptions> GlobalParameters
        {
            get
            {
                return RecognizesConfiguration.Properties.Select(p =>
                    new ArgumentWithOptions(
                        type: p.Type,
                        argument: p.Name,
                        action: p.Action,
                        required: p.Required,
                        description: p.Description
                        )).ToList();
            }
        }

        public String Help()
        {
            var cout = new StringWriter(CultureInfo);
            Parse(new[] { Conventions.Help }).Invoke(cout);
            return cout.ToString();
        }

        public ControllerExpression Controller(string controllerName)
        {
            return new ControllerExpression(controllerName, this);
        }
    }
}
