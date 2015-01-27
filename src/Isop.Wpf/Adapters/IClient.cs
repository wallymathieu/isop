
using Isop.Client.Models;
using Isop.Gui.ViewModels;
using System.Threading.Tasks;
namespace Isop.Gui.Adapters
{
    public interface IClient
    {
        Task<IReceiveResult> Invoke(Root root, Method method, IReceiveResult result);

        Task<Root> GetModel();
    }
}
