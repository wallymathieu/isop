using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isop.Gui
{
    public interface IJSonHttpClient
    {
        Task<JsonResponse> Request(Request request);
    }
}
