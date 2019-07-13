using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Isop.CommandLine
{
    using Infrastructure;
    using Lex;
    using Parse;
    using Domain;
    using Microsoft.Extensions.Options;

    internal class ControllerRecognizer
    {
        private readonly bool _allowInferParameter;
        private readonly IOptions<Configuration> _configuration;
        private readonly IServiceProvider _typeContainer;
        private readonly ConvertArgumentsToParameterValue _convertArgument;
        private readonly Formatter _formatter;
        /// <summary>
        /// </summary>
        public ControllerRecognizer(
            IOptions<Configuration> configuration,
            IServiceProvider typeContainer,
            TypeConverterFunc typeConverterFunc,
            Formatter formatter)
        {
            _configuration = configuration;
            _allowInferParameter = ! (_configuration?.Value?.DisableAllowInferParameter??false);
            _typeContainer = typeContainer;
            _formatter = formatter;
            _convertArgument = new ConvertArgumentsToParameterValue(configuration, typeConverterFunc);
        }

        private CultureInfo Culture => _configuration?.Value.CultureInfo ?? CultureInfo.CurrentCulture;

        public bool Recognize(Controller controller, IEnumerable<string> arg)
        {
            var lexed = RewriteLexedTokensToSupportHelpAndIndex.Rewrite(ArgumentLexer.Lex(arg).ToList());
            return null != FindMethodInfo(controller, lexed);
        }

        private Method FindMethodInfo(Controller controller, IList<Token> arg)
        {
            var foundClassName = controller.Name.EqualsIgnoreCase(arg.ElementAtOrDefault(0).Value);
            if (foundClassName)
            {
                var methodName = arg.ElementAtOrDefault(1).Value;
                var methodInfo = FindMethodAmongLexedTokens.FindMethod(controller.GetControllerActionMethods(), methodName, arg);
                return methodInfo;
            }
            return null;
        }

        /// <summary>
        /// Note that in order to register a converter you can use:
        /// TypeDescriptor.AddAttributes(typeof(AType), new TypeConverterAttribute(typeof(ATypeConverter)));
        /// </summary>
        private ParsedMethod Parse(Controller controller, IEnumerable<string> arg)
        {
            var lexed = RewriteLexedTokensToSupportHelpAndIndex.Rewrite(ArgumentLexer.Lex(arg).ToList());

            var methodInfo = FindMethodInfo(controller, lexed);
            if (methodInfo == null)
            {
                throw new Exception($"Could not find method for {controller.Name} with arguments {string.Join(" ", arg)}");
            }
            var argumentRecognizers = methodInfo.GetArguments()
                .ToList();
            argumentRecognizers.InsertRange(0, new[] { 
                new ArgumentWithOptions(ArgumentParameter.Parse("#0" + controller.Name, Culture), required: true, type: typeof(string)),
                new ArgumentWithOptions(ArgumentParameter.Parse("#1" + methodInfo.Name, Culture), required: false, type: typeof(string))
            });

            var parser = new ArgumentParser(argumentRecognizers, _allowInferParameter, Culture);
            var parsedArguments = parser.Parse(lexed, arg);

            return Parse(controller, methodInfo, parsedArguments);
        }

        private ParsedMethod Parse(Controller controller, Method methodInfo, ParsedArguments parsedArguments)
        {
            var unMatchedRequiredArguments = parsedArguments.UnMatchedRequiredArguments().ToArray();
            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments")
                          {
                              Arguments = unMatchedRequiredArguments
                                .Select(unmatched => unmatched.Name).ToArray()
                          };
            }
            var recognizedActionParameters = _convertArgument.GetParametersForMethod(methodInfo,
                parsedArguments.RecognizedArgumentsAsKeyValuePairs());

            return new ParsedMethod( parsedArguments, _typeContainer,
                recognizedClass: controller.Type,
                formatter: _formatter,
                recognizedAction: methodInfo,
                recognizedActionParameters: recognizedActionParameters);
        }

        public ParsedArguments ParseArgumentsAndMerge(Controller controller, IEnumerable<string> arg, ParsedArguments parsedArguments)
        {
            var parsedMethod = Parse(controller, arg);
            // Inferred ordinal arguments should not be recognized twice
            parsedArguments.RecognizedArguments = parsedArguments.RecognizedArguments
                .Where(argopts =>
                    !parsedMethod.RecognizedArguments.Any(pargopt => pargopt.Index == argopts.Index && argopts.InferredOrdinal));
            var merged = parsedArguments.Merge(parsedMethod);
            if (!controller.IgnoreGlobalUnMatchedParameters)
                merged.AssertFailOnUnMatched();
            return merged;
        }

        public ParsedArguments ParseArgumentsAndMerge(Controller controller, string actionName, Dictionary<string, string> arg, ParsedArguments parsedArguments)
        {
            var methodInfo = controller.GetMethod(actionName);
            var argumentRecognizers = methodInfo.GetArguments()
                .ToList();

            var parser = new ArgumentParser(argumentRecognizers, _allowInferParameter, Culture);
            var parsedMethodArguments = parser.Parse(arg);
            var parsedMethod = Parse(controller, methodInfo, parsedMethodArguments);
            var merged = parsedArguments.Merge(parsedMethod);
            if (!controller.IgnoreGlobalUnMatchedParameters)
                merged.AssertFailOnUnMatched();
            return merged;
        }
    }
}