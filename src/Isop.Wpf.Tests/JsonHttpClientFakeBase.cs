using Isop.Gui;
using System.Threading.Tasks;

namespace Isop.Wpf.Tests
{
    abstract class JsonHttpClientFakeBase : IJSonHttpClient
    {
        public virtual Task<JsonResponse> Request(Request request)
        {
            return Task.FromResult(RequestSync(request));
        }

        public abstract JsonResponse RequestSync(Request jsonRequest);
    }

}
