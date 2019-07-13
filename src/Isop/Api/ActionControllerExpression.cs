using System;
using Isop.CommandLine.Parse;
using System.Collections.Generic;
using System.Linq;
using Isop.Domain;
using Isop.Help;
using Microsoft.Extensions.DependencyInjection;

namespace Isop.Api
{
    /// <summary>
    /// 
    /// </summary>
    public class ActionControllerExpression
    {
        private readonly string _controllerName;
        private readonly string _actionName;
        private readonly AppHost _build;

        internal ActionControllerExpression(string controllerName, string actionName, AppHost build)
        {
            _controllerName = controllerName;
            _actionName = actionName;
            _build = build;
        }
        /// <summary>
        /// Get arguments for controller action
        /// </summary>
        public IEnumerable<Argument> GetArguments() =>
            _build.RecognizesConfiguration.Recognizes.SingleOrDefault(r => r.Recognize(_controllerName, _actionName))
                ?.GetMethod(_actionName).GetArguments() ?? throw new ArgumentException($"Controller: {_controllerName}, method: {_actionName}");
        /// <summary>
        /// send parameters to controller actions
        /// </summary>
        public ParsedArguments Parameters(Dictionary<string, string> parameters)
        {
            var argumentParser = new ArgumentParser(_build.GlobalParameters, _build.AllowInferParameter, _build.CultureInfo);
            var parsedArguments = argumentParser.Parse(parameters);
            if (_build.RecognizesConfiguration.Recognizes.Any())
            {
                var recognizedController = _build.RecognizesConfiguration.Recognizes
                    .FirstOrDefault(controller => controller.Recognize(_controllerName, _actionName));
                if (null != recognizedController)
                {
                    return _build.ControllerRecognizer.ParseArgumentsAndMerge(recognizedController, _actionName, parameters, parsedArguments);
                }
            }
            parsedArguments.AssertFailOnUnMatched();
            return parsedArguments;
        }
        /// <summary>
        /// Get help for controller action
        /// </summary>
        public string Help()
        {
            var helpController= _build.ServiceProvider.GetRequiredService<HelpController>();
            return (helpController.Index(_controllerName, _actionName) ?? String.Empty).Trim();
        }
    }
}
