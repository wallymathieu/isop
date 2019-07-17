using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Isop.CommandLine
{
    using Infrastructure;
    using Lex;
    using Parse;
    using Domain;
    using Abstractions;

    internal class ControllerRecognizer
    {
        private readonly bool _allowInferParameter;
        private readonly IOptions<Configuration> _configuration;
        private readonly Conventions _conventions;
        private readonly ConvertArgumentsToParameterValue _convertArgument;
        /// <summary>
        /// controller name -> controller, action name -> action
        /// </summary>
        private IDictionary<string, (Controller, ILookup<string, Method>)> _controllerActionMap;
        
        public ControllerRecognizer(
            IOptions<Configuration> configuration,
            TypeConverter typeConverterFunc, 
            IOptions<Conventions> conventions,
            Recognizes recognizes)
        {
            _configuration = configuration;
            _conventions = conventions.Value ?? throw new ArgumentNullException(nameof(conventions));
            _allowInferParameter = ! (_configuration?.Value?.DisableAllowInferParameter??false);
            _convertArgument = new ConvertArgumentsToParameterValue(configuration, typeConverterFunc);
            _controllerActionMap = recognizes.Controllers
                .ToDictionary(
                    c => c.GetName(_conventions), 
                    c => (c, c.GetControllerActionMethods(_conventions)
                                    .ToLookup(m => m.Name, m => m, StringComparer.OrdinalIgnoreCase)), 
                    StringComparer.OrdinalIgnoreCase);
        }

        private CultureInfo Culture => _configuration?.Value?.CultureInfo;

        public bool TryRecognize(IEnumerable<string> arg, out (Controller,Method,IReadOnlyList<Token>) controllerAndMethodAndToken)
        {
            var lexed = RewriteLexedTokensToSupportHelpAndIndex.Rewrite(_conventions,ArgumentLexer.Lex(arg).ToList());
            if (_controllerActionMap.TryGetValue(lexed.ElementAtOrDefault(0).Value,
                out var controllerAndMap))
            {
                var (controller,methodMap)= controllerAndMap;
                var method = FindMethodAmongLexedTokens.FindMethod(methodMap, lexed.ElementAtOrDefault(1).Value, lexed);
                if (method != null)
                {
                    controllerAndMethodAndToken = (controller, method, lexed);
                    return true;
                }
            }
            controllerAndMethodAndToken = default;
            return false;
        }

        public bool TryFind(string controllerName, string actionName,
            out (Controller, Method) controllerAndMethod)
        {
            if (_controllerActionMap.TryGetValue(controllerName,
                out var controllerAndMap))
            {
                var (controller,methodMap)= controllerAndMap;
                var method = FindMethodAmongLexedTokens.FindMethod(methodMap, actionName, new Token[0]);
                if (method != null)
                {
                    controllerAndMethod = (controller, method);
                    return true;
                }
            }
            controllerAndMethod = default;
            return false;
        }

        public ParsedArguments Parse(Controller controller, Method method, IReadOnlyList<Token> controllerLexed, IReadOnlyCollection<string> arg)
        {
            var argumentRecognizers = method.GetArguments(Culture)
                .ToList();
            argumentRecognizers.InsertRange(0, new[] { 
                new Argument(parameter: ArgumentParameter.Parse("#0" + controller.GetName(_conventions), Culture), required: true),
                new Argument(parameter: ArgumentParameter.Parse("#1" + method.Name, Culture), required: false)
            });

            var parser = new ArgumentParser(argumentRecognizers, _allowInferParameter);
            var parsedArguments = parser.Parse(controllerLexed, arg);

            var recognizedActionParameters = _convertArgument.GetParametersForMethod(method,
                parsedArguments.Recognized
                .Select(a => new KeyValuePair<string,string>(a.RawArgument, a.Value))
                .ToArray());

            return new ParsedArguments.Method(
                recognizedActionParameters: recognizedActionParameters,
                recognizedClass: controller.Type,
                recognizedAction: method);
        }
    }
}