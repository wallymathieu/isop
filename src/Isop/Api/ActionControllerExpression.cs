using System;
using Isop.CommandLine.Parse;
using System.Collections.Generic;
using System.Linq;

namespace Isop.Api
{
    
    public class ActionControllerExpression
    {
        private string controllerName;
        private string actionName;
        private Build build;

        public ActionControllerExpression(string controllerName, string actionName, Build build)
        {
            this.controllerName = controllerName;
            this.actionName = actionName;
            this.build = build;
        }
        public ParsedArguments Parameters(Dictionary<string, string> arg)
        {
            var argumentParser = new ArgumentParser(build.GlobalParameters, build.AllowInferParameter, build.CultureInfo);
            var parsedArguments = argumentParser.Parse(arg);
            if (build.ControllerRecognizers.Any())
            {
                var recognizers = build.ControllerRecognizers.Select(cr => cr.Value());
                var controllerRecognizer = recognizers.FirstOrDefault(recognizer => recognizer.Recognize(controllerName, actionName));
                if (null != controllerRecognizer)
                {
                    return controllerRecognizer.ParseArgumentsAndMerge(actionName, arg,
                        parsedArguments);
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
            this.build.HelpController();
            if (this.build.HelpForControllers != null)
            {
                var controller = this.build.Recognizes.Single(c => c.Recognize(controllerName, actionName));
                var method = controller.GetMethod(actionName);
                return (this.build.HelpForControllers.Description(controller, method) ?? String.Empty).Trim();
            }
            return null;
        }

    }
}
