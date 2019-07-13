using Isop.Domain;

namespace Isop.Api
{
    public class ControllerExpression
    {
        private readonly string _controllerName;
        private readonly ParserWithConfiguration _build;
        internal ControllerExpression(string controllerName, ParserWithConfiguration build)
        {
            _controllerName = controllerName;
            _build = build;
        }
        public ActionControllerExpression Action(string actionName) =>
            new ActionControllerExpression(_controllerName, actionName, _build);
    }
}

