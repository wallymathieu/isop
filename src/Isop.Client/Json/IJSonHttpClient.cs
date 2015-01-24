using System.Threading.Tasks;

namespace Isop.Client.Json
{
    public interface IJSonHttpClient
    {
        Task<JsonResponse> Request(Request request);
    }
}
