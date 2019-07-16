﻿using System;
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
        private readonly IOptions<Configuration> _options;
        private HelpController _helpController;
        private readonly IOptions<Texts> _texts;
        
        internal HelpController HelpController =>
            _helpController??(_helpController = ServiceProvider.GetService<HelpController>() ?? 
                                                new HelpController(
                                                    _texts,
                                                    Recognizes,
                                                    _options,
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
            _options = options ?? Options.Create(new Configuration());
            Recognizes = recognizes;
            ServiceProvider = serviceProvider;
            Conventions = conventions?? Options.Create(new Conventions());
            ControllerRecognizer = new ControllerRecognizer(options,
                typeConverter, Conventions);
            _texts = texts ?? Options.Create(new Texts());
        }
        /// <summary>
        /// Parse command line arguments and return parsed arguments entity
        /// </summary>
        public IParsedExpression Parse(IEnumerable<string> arg) => Parse(arg.ToList());

        private IParsedExpression Parse(IReadOnlyCollection<string> arg)
        {
            var argumentParser = new ArgumentParser(
                Recognizes.Properties.Select(p=>p.ToArgument(_options.Value.CultureInfo)).ToArray(),
                AllowInferParameter);
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
