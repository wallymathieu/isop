using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Isop.Controller;
using Isop.Infrastructure;

namespace Isop.WpfControls.ViewModels
{
    public static class BuildExtensions
    {
        public static ParsedMethod Parse(this Build that, Method currentMethod)
        {
            var controllerRecognizer = that.ControllerRecognizers
                .First(c => c.Key.ControllerName().Equals(currentMethod.ClassName));

            var parsedArguments = currentMethod.Parameters.GetParsedArguments();
            var unMatchedRequiredArguments = parsedArguments.UnMatchedRequiredArguments();
            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments")
                          {
                              Arguments = unMatchedRequiredArguments
                                  .Select(unmatched => new KeyValuePair<string, string>(unmatched.Argument.ToString(), unmatched.Argument.Help())).ToList()
                          };
            }

            var method = controllerRecognizer.Key.GetControllerActionMethods()
                .First(m => m.Name.Equals(currentMethod.Name));
            var parsedMethod = controllerRecognizer.Value().Parse(method, parsedArguments);
            parsedMethod.Factory = that.GetFactory();
            return parsedMethod;
        }

        public static MethodTreeModel GetMethodTreeModel(this Build that)
        {
            return new MethodTreeModel(globalParameters:new List<Param>(
                                           that.GlobalParameters
                                               .Select(p => new Param(typeof(string), p.Argument.ToString(), p)))
                                           .ToArray(),
                                       controllers:that.Recognizes
                                           .Where(cmr => !cmr.ControllerName().EqualsIC("help"))
                                           .Select(cmr => Controller(that, cmr)).ToArray(),
                                       build:that);
        }

        private static Controller Controller(Build that, Type type)
        {
            return new Controller
                       {
                           Name = type.ControllerName(),
                           Methods = type.GetControllerActionMethods().Select(m => Method(that, type, m)).ToArray()
                       };
        }

        private static Method Method(Build that, Type type, MethodInfo m)
        {
            var t = new TurnParametersToArgumentWithOptions(that.CultureInfo, that.TypeConverter);
            var @params = t.GetRecognizers(m).Select(p => new Param(p.Type, p.Argument.Prototype, p));
            return new Method(m.Name, type.ControllerName(), that.HelpController())
                       {
                           Parameters = new List<Param>(@params.ToArray())
                       };
        }
    }
}