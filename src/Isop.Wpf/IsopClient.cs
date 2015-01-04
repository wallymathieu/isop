using Isop.Gui.Models;
using Isop.Gui.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Isop.Gui
{
    public class IsopClient : IIsopClient, IDisposable
    {
        private IJSonHttpClient JsonClient;
        private string BasePath;
        public IsopClient(IJSonHttpClient jsonClient, string basePath)
        {
            JsonClient = jsonClient;
            BasePath = basePath;
        }

        public void Dispose()
        {
        }

        public async Task<Models.Root> GetModel()
        {
            var response = await JsonClient.Request(new Request(BasePath + "/", r => r.Get()));
            return JsonConvert.DeserializeObject<Models.Root>(response.Data);
        }

        public async Task<IReceiveResult> Invoke(Models.Method method, IEnumerable<Models.Param> globalParameters, IReceiveResult result)
        {
            var form = method.Parameters.ToDictionary(p => p.Name, p => p.Value);
            foreach (var global in globalParameters)
            {
                if (!form.ContainsKey(global.Name))
                {
                    form.Add(global.Name, global.Value);
                }
            }
            using (var rstream = await JsonClient.Request(new Request(BasePath + method.Url, r => r.Post().Form(form).Stream())))
            {
                if (null != rstream.Stream)
                    using (var reader = new StreamReader(rstream.Stream, Encoding.UTF8))
                    {
                        while (true)
                        {
                            var line = await reader.ReadLineAsync();
                            if (line == null)
                            {
                                break;
                            }
                            result.Result += line;
                        }
                    }
                if (null != rstream.RequestException)
                {
                    switch (rstream.RequestException.ErrorType)
                    {
                        case "TypeConversionFailed":
                            {
                                var err = JsonConvert.DeserializeObject<TypeConversionFailed>(rstream.RequestException.Message);
                                result.Error = err;
                                result.ErrorMessage = err.Message;
                            }
                            break;
                        case "MissingArgument":
                            {
                                var err = JsonConvert.DeserializeObject<MissingArgument>(rstream.RequestException.Message);
                                result.Error = err;
                                result.ErrorMessage = err.Message;
                            }
                            break;
                        default:// internal server error 
                            throw new Exception(rstream.RequestException.ErrorType);
                    }
                }
                return result;
            }
        }

        [Obsolete("Should not really be here. Should be inlined in tests instead.")]
        public async Task<RootViewModel> GetMethodTreeModel()
        {
            var mt = await GetModel();
            return new RootViewModel(this, mt.GlobalParameters.ToArray(), mt.Controllers.ToArray());
        }
    }
}
