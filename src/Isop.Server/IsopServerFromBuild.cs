using System;
using System.Collections.Generic;
using System.Linq;
using Isop.Infrastructure;
using Isop.Server.Models;
using System.Reflection;

namespace Isop.Server
{
    public class IsopServerFromBuild : IIsopServer
    {
        Func<Build> Build;

        public IsopServerFromBuild(Func<Build> build)
        {
            Build = build;
        }

        public Models.Controller GetController(string controller)
        {
            return GetModel().Controllers.Single(c => c.Name.EqualsIC(controller));
        }

        public Models.Method GetControllerMethod(string controller, string method)
        {
            return GetController(controller).Methods.Single(m => m.Name.EqualsIC(method));
        }

        public Models.MethodTreeModel GetModel()
        {
            using (var build = Build())
            {
                return Map.GetMethodTreeModel(build);
            }
        }

        public IEnumerable<string> InvokeMethod(Models.Method method, IDictionary<string, object> form)
        {
            using (var build = Build())
            {
                return build
                    .Controller(method.ClassName)
                    .Action(method.Name)
                    .Parameters(form.ToDictionary(p => p.Key, p => p.Value != null ? p.Value.ToString() : (string)null))
                    .Invoke();
            }
        }

        private class Map
        {
            public static MethodTreeModel GetMethodTreeModel(Build that)
            {
                return new MethodTreeModel(globalParameters: new List<Param>(
                                               that.GlobalParameters
                                                   .Select(p => new Param(typeof(string), p.Argument.ToString(), p.Required)))
                                               .ToArray(),
                                           controllers: that.Recognizes
                                             .Where(cmr => !cmr.IsHelp())
                    .Select(cmr => Controller(that, cmr)).ToArray());
            }

            private static Isop.Server.Models.Controller Controller(Build that, Isop.Domain.Controller type)
            {
                return new Isop.Server.Models.Controller(type.Name, type.GetControllerActionMethods().Select(m => Method(that, type, m)).ToArray());
            }

            private static Method Method(Build that, Isop.Domain.Controller type, Isop.Domain.Method m)
            {
                var @params = m.GetArguments().Select(p => new Param(p.Type, p.Name, p.Required)).ToArray();

                var helpController = that.HelpController();
                var help = helpController != null ? helpController.Index(type.Name, m.Name) : null;

                return new Method(m.Name, type.Name, help)
                {
                    Parameters = new List<Param>(@params.ToArray())
                };
            }

        }
    }
}
