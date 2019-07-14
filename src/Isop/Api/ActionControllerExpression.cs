using System;
using Isop.CommandLine.Parse;
using System.Collections.Generic;
using System.Linq;
using Isop.Domain;
using Isop.CommandLine;
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
            _build.Recognizes.Controllers.SingleOrDefault(r => r.Recognize(_controllerName, _actionName))
                ?.GetMethod(_actionName).GetArguments(_build.CultureInfo) ?? throw new ArgumentException($"Controller: {_controllerName}, method: {_actionName}");
        /// <summary>
        /// send parameters to controller actions
        /// </summary>
        public ParsedExpression Parameters(Dictionary<string, string> parameters)
        {
            var argumentParser = new ArgumentParser(_build.Recognizes.Properties.Select(p=>p.ToArgument(_build.CultureInfo)), _build.AllowInferParameter, _build.CultureInfo);
            var parsedArguments = argumentParser.Parse(parameters);
            if (_build.Recognizes.Controllers.Any())
            {
                var recognizedController = _build.Recognizes.Controllers
                    .FirstOrDefault(controller => controller.Recognize(_controllerName, _actionName));
                if (null != recognizedController)
                {
                    return new ParsedExpression( _build.ControllerRecognizer.ParseArgumentsAndMerge(recognizedController, _actionName, parameters, parsedArguments), _build);
                }
            }
            parsedArguments.AssertFailOnUnMatched();
            return new ParsedExpression(parsedArguments, _build);
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
