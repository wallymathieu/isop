using System;
using Isop.CommandLine.Parse;
using System.Collections.Generic;
using System.Linq;
using Isop.Domain;
using Isop.Help;
using Microsoft.Extensions.DependencyInjection;

namespace Isop.Api
{
    
    public class ActionControllerExpression
    {
        private string controllerName;
        private string actionName;
        private AppHost build;

        internal ActionControllerExpression(string controllerName, string actionName, AppHost build)
        {
            this.controllerName = controllerName;
            this.actionName = actionName;
            this.build = build;
        }
        internal IEnumerable<Argument> GetArguments() =>
            build.RecognizesConfiguration.Recognizes.SingleOrDefault(r => r.Recognize(controllerName, actionName))
                ?.GetMethod(actionName).GetArguments() ?? throw new ArgumentException($"Controller: {controllerName}, method: {actionName}");

        public ParsedArguments Parameters(Dictionary<string, string> arg)
        {
            var argumentParser = new ArgumentParser(build.GlobalParameters, build.AllowInferParameter, build.CultureInfo);
            var parsedArguments = argumentParser.Parse(arg);
            if (build.RecognizesConfiguration.Recognizes.Any())
            {
                var recognizedController = build.RecognizesConfiguration.Recognizes
                    .FirstOrDefault(controller => controller.Recognize(controllerName, actionName));
                if (null != recognizedController)
                {
                    return build.ControllerRecognizer.ParseArgumentsAndMerge(recognizedController, actionName, arg, parsedArguments);
                }
            }
            parsedArguments.AssertFailOnUnMatched();
            return parsedArguments;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Help()
        {
            var helpForControllers= build.ServiceProvider.GetRequiredService<HelpForControllers>();
            var controller = this.build.RecognizesConfiguration.Recognizes.Single(c => c.Recognize(controllerName, actionName));
            var method = controller.GetMethod(actionName);
            return (helpForControllers.Description(controller, method) ?? String.Empty).Trim();
        }
    }
}
