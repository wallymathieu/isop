using System;
using Isop.CommandLine.Parse;
using System.Collections.Generic;
using System.Linq;

namespace Isop.Api
{
    public class ControllerExpression
    {
        private string controllerName;
        private Build build;

        public ControllerExpression(string controllerName, Build build)
        {
            this.controllerName = controllerName;
            this.build = build;
        }
        public ActionControllerExpression Action(string actionName)
        {
            return new ActionControllerExpression(controllerName, actionName, build);
        }
    }
}

