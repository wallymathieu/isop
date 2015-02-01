using Isop.Client;
using Isop.Gui.ViewModels;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Isop.Client.Json;

namespace Isop.Gui.Adapters
{
    public class JsonClient : IClient
    {
        private IIsopClient Client;
        public JsonClient(IIsopClient client)
        {
            this.Client = client;
        }

        public async Task<IReceiveResult> Invoke(Isop.Client.Models.Root root, Isop.Client.Models.Method method, IReceiveResult result)
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

        public async Task<Isop.Client.Models.Root> GetModel()
        {
            return await this.Client.GetModel();
        }
    }
}
