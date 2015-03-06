using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Isop.CommandLine
{
    using Infrastructure;
    using Lex;
    using Parse;
    using Domain;

    public class ControllerRecognizer
    {
        private readonly bool _allowInferParameter;
        private readonly Configuration _configuration;
        private readonly Controller _controller;
        private readonly TypeContainer _typeContainer;
        /// <summary>
        /// </summary>
        public ControllerRecognizer(Controller controller,
            Configuration configuration,
            TypeContainer typeContainer,
            bool allowInferParameter = false)
        {
            _controller = controller;
            _configuration = configuration;
            _allowInferParameter = allowInferParameter;
            _typeContainer = typeContainer; 
        }

        public IEnumerable<Argument> GetRecognizers(string methodname)
        {//For tests mostly
            return _controller.GetMethod(methodname).GetArguments();
        }

        private CultureInfo Culture { get { return _configuration.CultureInfo ?? CultureInfo.CurrentCulture; } }

        public bool Recognize(IEnumerable<string> arg)
        {
            var lexed = RewriteLexedTokensToSupportHelpAndIndex.Rewrite(ArgumentLexer.Lex(arg).ToList());
            return null != FindMethodInfo(lexed);
        }

        private Method FindMethodInfo(IList<Token> arg)
        {
            var foundClassName = _controller.Name.EqualsIC(arg.ElementAtOrDefault(0).Value);
            if (foundClassName)
            {
                var methodName = arg.ElementAtOrDefault(1).Value;
                var methodInfo = FindMethodAmongLexedTokens.FindMethod(_controller.GetControllerActionMethods(), methodName, arg);
                return methodInfo;
            }
            return null;
        }

        /// <summary>
        /// Note that in order to register a converter you can use:
        /// TypeDescriptor.AddAttributes(typeof(AType), new TypeConverterAttribute(typeof(ATypeConverter)));
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public ParsedMethod Parse(IEnumerable<string> arg)
        {
            var lexed = RewriteLexedTokensToSupportHelpAndIndex.Rewrite(ArgumentLexer.Lex(arg).ToList());

            var methodInfo = FindMethodInfo(lexed);

            var argumentRecognizers = methodInfo.GetArguments()
                .ToList();
            argumentRecognizers.InsertRange(0, new[] { 
                new ArgumentWithOptions(ArgumentParameter.Parse("#0" + _controller.Name, Culture), required: true, type: typeof(string)),
                new ArgumentWithOptions(ArgumentParameter.Parse("#1" + methodInfo.Name, Culture), required: false, type: typeof(string))
            });

            var parser = new ArgumentParser(argumentRecognizers, _allowInferParameter, Culture);
            var parsedArguments = parser.Parse(lexed, arg);

            return Parse(methodInfo, parsedArguments);
        }

        public ParsedMethod Parse(Method methodInfo, ParsedArguments parsedArguments)
        {
            var unMatchedRequiredArguments = parsedArguments.UnMatchedRequiredArguments();
            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments")
                          {
                              Arguments = unMatchedRequiredArguments
                                .Select(unmatched => unmatched.Name).ToArray()
                          };
            }
            var convertArgument = new ConvertArgumentsToParameterValue(_configuration.CultureInfo, _configuration.TypeConverter);
            var recognizedActionParameters = convertArgument.GetParametersForMethod(methodInfo,
                parsedArguments.RecognizedArgumentsAsKeyValuePairs());

            return new ParsedMethod( parsedArguments, _typeContainer, _configuration)
                       {
                           RecognizedAction = methodInfo,
                           RecognizedActionParameters = recognizedActionParameters,
                           RecognizedClass = _controller.Type
                       };
        }

        public ParsedArguments ParseArgumentsAndMerge(IEnumerable<string> arg, ParsedArguments parsedArguments)
        {
            var parsedMethod = Parse(arg);
            // Inferred ordinal arguments should not be recognized twice
            parsedArguments.RecognizedArguments = parsedArguments.RecognizedArguments
                .Where(argopts =>
                    !parsedMethod.RecognizedArguments.Any(pargopt => pargopt.Index == argopts.Index && argopts.InferredOrdinal));
            var merged = parsedArguments.Merge(parsedMethod);
            if (!_controller.IgnoreGlobalUnMatchedParameters)
                merged.AssertFailOnUnMatched();
            return merged;
        }

        public bool Recognize(string controllerName, string actionName)
        {
            return _controller.Recognize(controllerName, actionName);
        }

        public ParsedArguments ParseArgumentsAndMerge(string actionName, Dictionary<string, string> arg, ParsedArguments parsedArguments)
        {

            var methodInfo = _controller.GetMethod(actionName);
            var argumentRecognizers = methodInfo.GetArguments()
                .ToList();

            var parser = new ArgumentParser(argumentRecognizers, _allowInferParameter, Culture);
            var parsedMethodArguments = parser.Parse(arg);
            var parsedMethod = Parse(methodInfo, parsedMethodArguments);
            var merged = parsedArguments.Merge(parsedMethod);
            if (!_controller.IgnoreGlobalUnMatchedParameters)
                merged.AssertFailOnUnMatched();
            return merged;
        }
    }
}