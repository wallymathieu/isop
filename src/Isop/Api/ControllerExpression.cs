using System;
using Isop.CommandLine.Parse;
using System.Collections.Generic;
using System.Linq;

namespace Isop.Api
{
    public class ControllerExpression
    {
        private readonly string _controllerName;
        private readonly Build _build;

        public ControllerExpression(string controllerName, Build build)
        {
            this._controllerName = controllerName;
            this._build = build;
        }
        public ActionControllerExpression Action(string actionName)
        {
            return new ActionControllerExpression(_controllerName, actionName, _build);
        }
    }
}

