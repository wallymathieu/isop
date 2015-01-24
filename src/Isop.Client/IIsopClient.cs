using Isop.Client.Json;
using Isop.Client.Models;
using System;
using System.Threading.Tasks;

namespace Isop.Client
{
    public interface IIsopClient
    {
        Task<Root> GetModel();
        Task<JsonResponse> Invoke(Root root, Method method, Func<Request.Configure, Request.Configure> action);
    }
}
