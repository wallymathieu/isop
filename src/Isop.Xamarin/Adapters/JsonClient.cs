using Isop.Client;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Isop.Client.Json;

namespace Isop.Xamarin
{
    public class JsonClient : IClient
    {
        private IIsopClient Client;
        public JsonClient(IIsopClient client)
        {
            this.Client = client;
        }

        public async Task<IReceiveResult> Invoke(Client.Transfer.Root root, Client.Transfer.Method method, IReceiveResult result)
        {
            try
            {
                result.Result = String.Empty;
                result.Errors = null;
                using (var rstream = await Client.Invoke(root, method, r => r.Stream()))
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

                    return result;
                }
            }
            catch (AggregateException aggEx)
            {
                if (aggEx.InnerExceptions.Count == 1 && aggEx.InnerExceptions.Any(e => e is RequestException))
                {
                    var requestException = (RequestException)aggEx.InnerExceptions.Single();
                    var errorObject = requestException.ErrorObjects();
                    if (null != errorObject)
                    {
                        result.Errors = errorObject;
                        return result;
                    }
                }
                throw;
            }
            catch (RequestException ex)
            {
                var errorObject = ex.ErrorObjects();
                if (null != errorObject)
                {
                    result.Errors = errorObject;
                    return result;
                }
                throw;
            }
        }

        public static bool CanLoad(string url)
        {
            try
            {
                var uri = new Uri(url);
                switch (uri.Scheme)
                {
                    case "http":
                    case "https":
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Client.Transfer.Root> GetModel()
        {
            return await Client.GetModel();
        }
    }
}
