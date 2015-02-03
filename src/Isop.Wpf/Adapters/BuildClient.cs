using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isop.Client.Transfer;
using System;
using System.IO;

namespace Isop.Gui.Adapters
{
    public class BuildClient : IClient
    {
        private Build Build;

        public BuildClient(Build build)
        {
            this.Build = build;
        }
        public async Task<ViewModels.IReceiveResult> Invoke(Root root, Method method, ViewModels.IReceiveResult result)
        {
            result.Result = "";
            try
            {
                var parsed = Build.Controller(method.ClassName)
                               .Action(method.Name)
                               .Parameters(method.Parameters.ToDictionary(p => p.Name, p => p.Value));
                var returned = parsed.Invoke();

                foreach (var yielded in returned)
                {
                    result.Result += yielded;
                }
            }
            catch (TypeConversionFailedException ex)
            {
                var errorObject = new TypeConversionFailed
                {
                    Message = ex.Message,
                    Argument = ex.Argument,
                    TargetType = ex.TargetType.ToString(),
                    Value = ex.Value
                };
                result.Errors = new[] { errorObject };
            }
            catch (MissingArgumentException ex)
            {
                result.Errors = ex.Arguments
                    .Select(a => new MissingArgument { Message = ex.Message, Argument = a })
                    .ToArray();
            }
            return result;
        }

        public static bool CanLoad(string url)
        {
            return File.Exists(url);
        }

        public async Task<Client.Transfer.Root> GetModel()
        {
            return Map.GetMethodTreeModel(Build);
        }

        private class Map
        {
            public static Root GetMethodTreeModel(Build that)
            {
                return new Root
                {
                    GlobalParameters = new List<Param>(
                     that.GlobalParameters
                         .Select(p => new Param { Type = typeof(string).FullName, Name = p.Argument.ToString(), Required = p.Required }))
                     .ToArray(),
                    Controllers = that.Recognizes
                      .Where(cmr => !cmr.IsHelp())
                      .Select(cmr => Controller(that, cmr)).ToArray()
                };
            }

            private static Controller Controller(Build that, Isop.Domain.Controller type)
            {
                return new Controller
                {
                    Name = type.Name,
                    Methods = type.GetControllerActionMethods().Select(m => Method(that, type, m)).ToArray()
                };
            }

            private static Method Method(Build that, Isop.Domain.Controller type, Isop.Domain.Method m)
            {
                var @params = m.GetArguments().Select(p => new Param { Type = typeof(string).FullName, Name = p.Name, Required = p.Required }).ToArray();

                return new Method
                {
                    Name = m.Name,
                    ClassName = type.Name,
                    Help = that.Controller(type.Name).Action(m.Name).Help(),
                    Parameters = new List<Param>(@params.ToArray())
                };
            }
        }
    }
}
