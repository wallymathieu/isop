using System.Collections.Generic;
using System.Reflection;
using Isop.Parse;

namespace Isop.Controller
{
    public class MethodAndArguments
    {
        public MethodAndArguments(MethodInfo methodinfo, ControllerRecognizer cr)
        {
            Method = methodinfo;
            _cr = cr;
        }
        private readonly ControllerRecognizer _cr;
        public string Name { get { return Method.Name; } }

        public MethodInfo Method { get; private set; }
        public IEnumerable<ArgumentWithOptions> GetMethodArguments()
        {
            return _cr.GetRecognizers(Method);
        }
    }
}