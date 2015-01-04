﻿using System;
using System.Collections.Generic;
using With;
using With.Rubyfy;
using System.Linq;
using Isop.Infrastructure;
using Isop.Server.Models;
using System.Reflection;
using Isop.Controllers;

namespace Isop.Server
{
    public class IsopServerFromBuild : IIsopServer
    {
        Build Build;

        public IsopServerFromBuild(Build build)
        {
            Build = build;
        }

        public Models.Controller GetController(string controller)
        {
            return Map.GetMethodTreeModel(Build).Controllers.Single(c => c.Name.EqualsIC(controller));
        }

        public Models.Method GetControllerMethod(string controller, string method)
        {
            return GetController(controller).Methods.Single(m => m.Name.EqualsIC(method));
        }

        public Models.MethodTreeModel GetModel()
        {
            return Map.GetMethodTreeModel(Build);
        }

        public IEnumerable<string> InvokeMethod(Models.Method method, IDictionary<string, object> form)
        {
            return Build
                .Parse(
                    new List<string>() { { method.ClassName }, { method.Name } }
                    .Tap(l => l.AddRange(To(form))))
                .Invoke();
        }

        private IEnumerable<string> To(IDictionary<string, object> form)
        {
            return form.Map(kv => new string[] { "--" + kv.Key + "=", kv.Value.ToString() }).Flatten<string>();
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
                                               .Where(cmr => !cmr.ControllerName().EqualsIC("help"))
                                               .Select(cmr => Controller(that, cmr)).ToArray());
            }

            private static Isop.Server.Models.Controller Controller(Build that, Type type)
            {
                return new Isop.Server.Models.Controller(type.ControllerName(), type.GetControllerActionMethods().Select(m => Method(that, type, m)).ToArray());
            }

            private static Method Method(Build that, Type type, MethodInfo m)
            {
                var t = new TurnParametersToArgumentWithOptions(that.CultureInfo, that.TypeConverter);
                var @params = t.GetRecognizers(m).Select(p => new Param(p.Type, p.Argument.Prototype, p.Required)).ToArray();

                var help = that.HelpController().Yield(h => h != null ? h.Index(type.ControllerName(), m.Name) : null);

                return new Method(m.Name, type.ControllerName(), help)
                {
                    Parameters = new List<Param>(@params.ToArray())
                };
            }

        }
    }
}
