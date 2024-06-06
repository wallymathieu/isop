using System;
using System.Collections.Generic;
using System.Linq;
using Isop.Domain;

namespace Isop.Implementations
{
    using Abstractions;
    ///
    internal class ControllerExpression(string controllerName, AppHost appHost, Controller controller) : IController
    {
        public IActionOnController Action(string name) =>
            new ActionControllerExpression(Name, name, appHost);

        public IReadOnlyCollection<IActionOnController> Actions =>
            controller.GetControllerActionMethods(appHost.Conventions.Value).Select(action =>
                new ActionControllerExpression(Name, action.Name, appHost)).ToArray();

        public string Name { get; } = controllerName;

        public string Help()
        {
            var helpController = appHost.HelpController;
            return (helpController.Index(Name, null) ?? String.Empty).Trim();
        }
    }
}

