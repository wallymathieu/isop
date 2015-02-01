using Isop.Infrastructure;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Isop.Client.Models;
namespace Isop.Gui.Adapters
{
    class BuildClient : IClient
    {
        private Build Build;

        public BuildClient(Build build)
        {
            this.Build = build;
        }
        public async System.Threading.Tasks.Task<ViewModels.IReceiveResult> Invoke(Client.Models.Root root, Client.Models.Method method, ViewModels.IReceiveResult result)
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


        public async Task<Client.Models.Root> GetModel()
        {
            return Map.GetMethodTreeModel(Build);
        }

        private class Map
        {
            public static Client.Models.Root GetMethodTreeModel(Build that)
            {
                return new Client.Models.Root
                {
                    GlobalParameters = new List<Client.Models.Param>(
                     that.GlobalParameters
                         .Select(p => new Client.Models.Param { Type = typeof(string).FullName, Name = p.Argument.ToString(), Required = p.Required }))
                     .ToArray(),
                    Controllers = that.Recognizes
                      .Where(cmr => !cmr.IsHelp())
                      .Select(cmr => Controller(that, cmr)).ToArray()
                };
            }

            private static Client.Models.Controller Controller(Build that, Isop.Domain.Controller type)
            {
                return new Client.Models.Controller
                {
                    Name = type.Name,
                    Methods = type.GetControllerActionMethods().Select(m => Method(that, type, m)).ToArray()
                };
            }

            private static Client.Models.Method Method(Build that, Isop.Domain.Controller type, Isop.Domain.Method m)
            {
                var @params = m.GetArguments().Select(p => new Client.Models.Param { Type = typeof(string).FullName, Name = p.Name, Required = p.Required }).ToArray();

                return new Client.Models.Method
                {
                    Name = m.Name,
                    ClassName = type.Name,
                    Help = that.Controller(type.Name).Action(m.Name).Help(),
                    Parameters = new List<Client.Models.Param>(@params.ToArray())
                };
            }
        }
    }
}
