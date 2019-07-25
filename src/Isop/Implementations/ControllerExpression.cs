using System;
using System.Collections.Generic;
using System.Linq;
using Isop.Domain;

namespace Isop.Implementations
{
    using Abstractions;
    ///
    internal class ControllerExpression:IController
    {
        private readonly AppHost _appHost;
        private readonly Controller _controller;

        internal ControllerExpression(string controllerName, AppHost appHost, Controller controller)
        {
            Name = controllerName;
            _appHost = appHost;
            _controller = controller;
        }
        public IActionOnController Action(string name) =>
            new ActionControllerExpression(Name, name, _appHost);

        public IReadOnlyCollection<IActionOnController> Actions =>
            _controller.GetControllerActionMethods(_appHost.Conventions.Value).Select(action =>
                new ActionControllerExpression(Name, action.Name, _appHost)).ToArray();

        public string Name { get; }

        public string Help()
        {
            var helpController = _appHost.HelpController;
            return (helpController.Index(Name, null) ?? String.Empty).Trim();
        }
    }
}

