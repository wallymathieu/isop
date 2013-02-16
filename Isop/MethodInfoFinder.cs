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
        
        public IEnumerable<MethodInfoOrProperty> FindSet(IEnumerable<MethodInfo> methods,Type returnType=null, string name=null, IEnumerable<Type> parameters=null)
        {
            var retv = Find (methods,returnType,null,parameters)
                .Where(m=>m.Name.StartsWith("set", StringComparison.OrdinalIgnoreCase));
            if (null != name)
                retv = retv.Where (m => 
                    m.Name.Equals ("set" + name, StringComparison.OrdinalIgnoreCase)
                    || m.Name.Equals ("set_" + name, StringComparison.OrdinalIgnoreCase));
            return retv.Select(m=> {
                if (m.Name.StartsWith("set_"))
                {
                    return new MethodInfoOrProperty(m, m.DeclaringType.GetProperty(m.Name.Replace("set_",""))); // can prob be optimized
                }
                return new MethodInfoOrProperty(m); 
            });
        }
            
        public MethodInfo Match (IEnumerable<MethodInfo> methods, Type returnType=null, string name=null, IEnumerable<Type> parameters=null)
        {
            var retv = Find (methods, returnType, name, parameters);
            return retv.FirstOrDefault ();
        }

        public MethodInfoOrProperty MatchGet (IEnumerable<MethodInfo> methods, string name, Type returnType=null, IEnumerable<Type> parameters=null)
        {
            var retv = Find (methods, returnType, null, parameters);
                
            retv = retv.Where (m => m.Name.Equals (name, StringComparison.OrdinalIgnoreCase) 
                        || m.Name.Equals ("get_" + name, StringComparison.OrdinalIgnoreCase)
                        || m.Name.Equals ("get" + name, StringComparison.OrdinalIgnoreCase));
            var methodInfo = retv.FirstOrDefault ();
            if (null != methodInfo)
            {
                PropertyInfo propertyInfo=null;
                if (methodInfo.Name.StartsWith("get_"))
                    propertyInfo = methodInfo.DeclaringType.GetProperty(methodInfo.Name.Replace("get_", ""));
                return new MethodInfoOrProperty(methodInfo, propertyInfo);
            }
            else return null;
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

