using Isop.Client.Json;
using Isop.Client.Transfer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Isop.Client
{
    public class IsopClient : IIsopClient
    {
        private IJSonHttpClient JsonClient;
        private string BasePath;
        public IsopClient(IJSonHttpClient jsonClient, string basePath)
        {
            JsonClient = jsonClient;
            BasePath = basePath;
        }

        public async Task<Isop.Client.Transfer.Root> GetModel()
        {
            var response = await JsonClient.Request(new Request(BasePath, r => r.Get()));
            return JsonConvert.DeserializeObject<Isop.Client.Transfer.Root>(response.Data);
        }

        public async Task<JsonResponse> Invoke(Root root, Method method, Func<Request.Configure, Request.Configure> action)
        {
            var form = method.Parameters.ToDictionary(p => p.Name, p => p.Value);
            foreach (var global in root.GlobalParameters)
            {
                if (!form.ContainsKey(global.Name))
                {
                    form.Add(global.Name, global.Value);
                }
            }
            return await Invoke(method, form, action);
        }

        private async Task<JsonResponse> Invoke(Isop.Client.Transfer.Method method, Dictionary<string, string> form, Func<Request.Configure, Request.Configure> action)
        {
            var uri = new Uri(BasePath);
            var url = uri.Scheme + "://" + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);
            return await JsonClient.Request(new Request(url + method.Url, r => action(r.Post().Form(form))));
        }
    }
}
