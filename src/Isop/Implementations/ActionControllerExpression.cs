using System;
using System.Collections.Generic;
using System.Linq;
using Isop.Infrastructure;

namespace Isop.Implementations
{
    using Abstractions;
    using CommandLine;
    using CommandLine.Parse;
    using Infrastructure;

    internal class ActionControllerExpression:IActionControllerExpression
    {
        private readonly string _controllerName;
        private readonly string _actionName;
        private readonly AppHost _appHost;
        private readonly ConvertArgumentsToParameterValue _convertArguments;

        internal ActionControllerExpression(string controllerName, string actionName, AppHost appHost)
        {
            _controllerName = controllerName;
            _actionName = actionName;
            _appHost = appHost;
            _convertArguments = new ConvertArgumentsToParameterValue(_appHost.Configuration,_appHost.TypeConverter);
        }

        public IReadOnlyCollection<Argument> GetArguments() =>
            _appHost.ControllerRecognizer.TryFind(_controllerName, _actionName, out var controllerAndMethod)
                ? controllerAndMethod.Item2.GetArguments(_appHost.CultureInfo).ToArray()
                : throw new ArgumentException($"Controller: {_controllerName}, method: {_actionName}");
        public IParsedExpression Parameters(Dictionary<string, string> parameters)
        {
            var valuePairs = parameters.ToArray();
            
            var unrecognizedArguments = new List<UnrecognizedArgument>();
            var recognizedArguments = new List<RecognizedArgument>();
            for (int index = 0; index < valuePairs.Length; index++)
            {
                var current = valuePairs[index];
                var prop = _appHost.Recognizes.Properties.FirstOrDefault(p => p.Name.EqualsIgnoreCase(current.Key));
                if (prop != null)
                {
                    recognizedArguments.Add(new RecognizedArgument(
                        prop.ToArgument(_appHost.CultureInfo),
                        new []{index},
                        current.Key,
                        current.Value));
                }
                else
                {
                    unrecognizedArguments.Add(new UnrecognizedArgument(index, current.Key));
                }
            }
            
            var parsedArguments = new ParsedArguments.Properties(
                unrecognized: unrecognizedArguments,
                recognized: recognizedArguments
            );
            if (!_appHost.ControllerRecognizer.TryFind(_controllerName, _actionName, out var controllerAndMethod))
                return new ParsedExpression(parsedArguments, _appHost);
            var (controller, method) = controllerAndMethod;
            if (!_convertArguments.TryGetParametersForMethod(method, parameters, out var parametersForMethod, out var missingParameters))
                return new ParsedExpression(parsedArguments.Merge(new ParsedArguments.MethodMissingArguments(controller.Type,method,missingParameters)), _appHost);
            var paramMap = valuePairs
                .Select(Tuple.Create<KeyValuePair<string, string>, int>)
                .ToDictionary(t => t.Item1.Key, vp => vp);
            var recognized = method.GetArguments(_appHost.CultureInfo)
                .SelectMany(arg=> paramMap.TryGetValue(arg.Name, out var value) 
                    ? new[] {(arg, value)} 
                    : new (Argument,Tuple<KeyValuePair<string,string>,int>)[0])
                .Select(chosen=>
                {
                    var (arg, value) = chosen;
                    return new RecognizedArgument(arg, new []{value.Item2}, value.Item1.Key, value.Item1.Value);
                }).ToArray();
            return new ParsedExpression(
                parsedArguments.Merge(new ParsedArguments.Method(
                    recognizedActionParameters: parametersForMethod,
                    recognized: 
                        recognized,
                    recognizedClass: controller.Type, 
                    recognizedAction:method)), 
                _appHost);
        }
        public string Help() => 
            (_appHost.HelpController.Index(_controllerName, _actionName) ?? String.Empty).Trim();
    }
}