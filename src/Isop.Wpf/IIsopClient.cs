using Isop.Gui.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isop.Gui
{
    public interface IIsopClient
    {
        Task<Models.Root> GetModel();
        Task<IReceiveResult> Invoke(Models.Method method, IEnumerable<Models.Param> globalParameters, IReceiveResult result);
    }
}
