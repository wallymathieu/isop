using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
namespace Isop
{
    public class MethodInfoFinder
    {
        static IEnumerable<MethodInfo> Find (IEnumerable<MethodInfo> methods, Type returnType, string name, IEnumerable<Type> parameters)
        {
            IEnumerable<MethodInfo> retv = methods;
            if (null != returnType)
                retv = retv.Where (m => m.ReturnType == returnType);
            if (null != parameters)
                retv = retv.Where (m => m.GetParameters ().Select (p => p.ParameterType).SequenceEqual (parameters));
            if (null != name)
                retv = retv.Where (m => m.Name.Equals (name, StringComparison.OrdinalIgnoreCase));
            return retv;
        }
            
        public MethodInfo Match (IEnumerable<MethodInfo> methods, Type returnType=null, string name=null, IEnumerable<Type> parameters=null)
        {
            var retv = Find (methods, returnType, name, parameters);
            return retv.FirstOrDefault ();
        }

        public MethodInfo MatchGet (IEnumerable<MethodInfo> methods, string name, Type returnType=null, IEnumerable<Type> parameters=null)
        {
            var retv = Find (methods, returnType, null, parameters);
                
            retv = retv.Where (m => m.Name.Equals (name, StringComparison.OrdinalIgnoreCase) 
                        || m.Name.Equals ("get_" + name, StringComparison.OrdinalIgnoreCase)
                        || m.Name.Equals ("get" + name, StringComparison.OrdinalIgnoreCase));
            return retv.FirstOrDefault ();
        }

        public IEnumerable<MethodInfo> GetOwnPublicMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public| BindingFlags.Instance)
                .Where(m=>!m.DeclaringType.Equals(typeof(Object)))
                .Where(m => !m.Name.StartsWith("get_", StringComparison.OrdinalIgnoreCase)
                        && !m.Name.StartsWith("set_", StringComparison.OrdinalIgnoreCase))
                ;
        }
    }

}

