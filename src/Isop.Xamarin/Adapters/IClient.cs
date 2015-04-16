
using Isop.Client.Transfer;
using System.Threading.Tasks;
namespace Isop.Xamarin
{
    public interface IClient
    {
        Task<IReceiveResult> Invoke(Root root, Method method, IReceiveResult result);

        Task<Root> GetModel();
    }
}
