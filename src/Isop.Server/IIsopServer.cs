using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isop.Server
{
    public interface IIsopServer
    {
        Models.MethodTreeModel GetModel();
        Models.Controller GetController(string controller);
        Models.Method GetControllerMethod(string controller, string method);
        IEnumerable<string> InvokeMethod(Models.Method method, IDictionary<string,object> form);
    }
}
