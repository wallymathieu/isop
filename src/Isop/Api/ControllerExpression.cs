using System;
using System.Linq;
using Isop.Domain;
using Isop.Help;
using Microsoft.Extensions.DependencyInjection;

namespace Isop.Api
{
    ///
    public class ControllerExpression
    {
        private readonly string _controllerName;
        private readonly AppHost _build;
        internal ControllerExpression(string controllerName, AppHost build)
        {
            _controllerName = controllerName;
            _build = build;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">action name</param>
        /// <returns></returns>
        public ActionControllerExpression Action(string name) =>
            new ActionControllerExpression(_controllerName, name, _build);
        /// <summary>
        /// Get help for controller
        /// </summary>
        public string Help()
        {
            var helpController = _build.HelpController;
            return (helpController.Index(_controllerName, null) ?? String.Empty).Trim();
        }
    }
}

