using System.Collections.Generic;
using System.Reflection;
using Isop.Parse;

namespace Isop.Controller
{
    public class MethodAndArguments
    {
        private readonly TurnParametersToArgumentWithOptions _turnParametersToArgumentWithOptions;
        
        public MethodAndArguments(MethodInfo methodinfo, TurnParametersToArgumentWithOptions turnParametersToArgumentWithOptions)
        {
            Method = methodinfo;
            _turnParametersToArgumentWithOptions = turnParametersToArgumentWithOptions;
        }

        public string Name { get { return Method.Name; } }

        public MethodInfo Method { get; private set; }
        public IEnumerable<ArgumentWithOptions> GetMethodArgumentsRecognizers()
        {
            return _turnParametersToArgumentWithOptions.GetRecognizers(Method);
        }
    }
}