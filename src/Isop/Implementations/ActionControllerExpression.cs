using System;
using System.Collections.Generic;
using System.Linq;

namespace Isop.Implementations
{
    using Abstractions;
    using CommandLine;
    using CommandLine.Parse;

    internal class ActionControllerExpression:IActionControllerExpression
    {
        private readonly string _controllerName;
        private readonly string _actionName;
        private readonly AppHost _appHost;

        internal ActionControllerExpression(string controllerName, string actionName, AppHost appHost)
        {
            _controllerName = controllerName;
            _actionName = actionName;
            _appHost = appHost;
        }
        public IReadOnlyCollection<Argument> GetArguments() =>
            _appHost.Recognizes.Controllers.SingleOrDefault(r => r.Recognize(_controllerName, _actionName))
                ?.GetMethod(_actionName).GetArguments(_appHost.CultureInfo).ToArray() ?? throw new ArgumentException($"Controller: {_controllerName}, method: {_actionName}");
        public IParsedExpression Parameters(Dictionary<string, string> parameters)
        {
            var argumentParser = new ArgumentParser(_appHost.Recognizes.Properties
                .Select(p=>p.ToArgument(_appHost.CultureInfo)).ToArray(), _appHost.AllowInferParameter);
            var parsedArguments = argumentParser.Parse(parameters);
            if (_appHost.Recognizes.Controllers.Any())
            {
                var recognizedController = _appHost.Recognizes.Controllers
                    .FirstOrDefault(controller => controller.Recognize(_controllerName, _actionName));
                if (null != recognizedController)
                {
                    return new ParsedExpression( _appHost.ControllerRecognizer.ParseArgumentsAndMerge(recognizedController, _actionName, parameters, parsedArguments), _appHost);
                }
            }
            parsedArguments.AssertFailOnUnMatched();
            return new ParsedExpression(parsedArguments, _appHost);
        }
        public string Help()
        {
            var helpController= _appHost.HelpController;
            return (helpController.Index(_controllerName, _actionName) ?? String.Empty).Trim();
        }
    }
}
