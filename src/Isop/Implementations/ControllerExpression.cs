using System;

namespace Isop.Implementations
{
    using Abstractions;
    ///
    internal class ControllerExpression:IControllerExpression
    {
        private readonly string _controllerName;
        private readonly AppHost _appHost;
        internal ControllerExpression(string controllerName, AppHost appHost)
        {
            _controllerName = controllerName;
            _appHost = appHost;
        }
        public IActionControllerExpression Action(string name) =>
            new ActionControllerExpression(_controllerName, name, _appHost);
        public string Help()
        {
            var helpController = _appHost.HelpController;
            return (helpController.Index(_controllerName, null) ?? String.Empty).Trim();
        }
    }
}

