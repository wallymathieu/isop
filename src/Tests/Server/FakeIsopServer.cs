using System.Collections.Generic;
using With.Rubyfy;
using Isop.Server;
using Isop.Server.Models;

namespace Isop.Tests.Server
{
    class FakeIsopServer : IIsopServer
    {
        public MethodTreeModel GetModel()
        {
            return new MethodTreeModel(new[] { new Param(typeof(string), "Global", false) }, new[] { new Controller("My", new Method[0]) });
        }

        public Controller GetController(string controller)
        {
            return new Controller("My", new[] { "Action", "Fail", "ActionWithGlobalParameter", "ActionWithObjectArgument" }.Map(n => new Method(n, "My", "Help")));
        }

        public Method GetControllerMethod(string controller, string method)
        {
            return new Method(method, null, "Help") { Parameters = new[] { "value" }.Map(p => new Param(typeof(string), p, true)) };
        }

        public IEnumerable<string> InvokeMethod(Method method, IDictionary<string, object> form)
        {
            yield return "<p>Method: " + method.Name + "</p>";
            foreach (var item in form)
            {
                yield return "<p>" + item.Key + "=" + item.Value + "</p>";
            }
        }
    }
}
