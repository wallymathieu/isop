using System.Collections.Generic;
using System.Linq;
using Isop.Controller;
using Isop.Infrastructure;
using Isop.Parse;

namespace Isop.WpfControls.ViewModels
{
    public static class BuildExtensions
    {
        public static ParsedMethod Parse(this Build that, Method currentMethod)
        {
            var controllerRecognizer = that.ControllerRecognizers
                .First(c => c.ClassName().Equals(currentMethod.ClassName));

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

            var method = controllerRecognizer.GetMethods()
                .First(m => m.Name.Equals(currentMethod.Name));
            var parsedMethod = controllerRecognizer.Parse(method, parsedArguments);
            parsedMethod.Factory = that.GetFactory();
            return parsedMethod;
        }

        public static MethodTreeModel GetMethodTreeModel(this Build that)
        {
            return new MethodTreeModel(new List<Param>(
                                           that.GlobalParameters
                                               .Select(p => new Param(typeof(string), p.Argument.ToString(), p)))
                                           .ToArray(),
                                       that.ControllerRecognizers
                                           .Where(cmr => !cmr.ClassName().EqualsIC("help"))
                                           .Select(cmr => new Controller
                                                              {
                                                                  Name = cmr.ClassName(),
                                                                  Methods = cmr.GetMethods().Select(m => new Method(m.Name, cmr.ClassName(), that.HelpController())
                                                                                                             {
                                                                                                                 Parameters = new List<Param>(
                                                                                                                     cmr.GetMethodParameterRecognizers(m).Select(p =>
                                                                                                                                                                 new Param(p.Type, p.Argument.Prototype, p
                                                                                                                                                                     )).ToArray())
                                                                                                             }).ToArray()
                                                              }).ToArray(),
                                       that
                );
        }
    }
}