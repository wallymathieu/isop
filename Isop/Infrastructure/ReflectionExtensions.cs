using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using Isop.Controller;

namespace Isop.Infrastructure
{
    public static class ReflectionExtensions
    {
        public static bool Required(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), true).Any();
        }

        public static bool Required(this MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(typeof(RequiredAttribute), true).Any();
        }
        public static IEnumerable<MethodInfo> Matches(this IEnumerable<MethodInfo> methods, Type returnType, string name, IEnumerable<Type> parameters)
        {
            IEnumerable<MethodInfo> retv = methods;
            if (null != returnType)
                retv = retv.Where(m => m.ReturnType == returnType);
            if (null != parameters)
                retv = retv.Where(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameters));
            if (null != name)
                retv = retv.Where(m => m.Name.EqualsIC(name));
            return retv;
        }

        public static IEnumerable<MethodInfoOrProperty> FindSet(this IEnumerable<MethodInfo> methods,Type returnType=null, string name=null, IEnumerable<Type> parameters=null)
        {
            var retv = methods.Matches(returnType,null,parameters)
                .Where(m=>m.Name.StartsWithIC("set"));
            if (null != name)
                retv = retv.Where (m => 
                                   m.Name.EqualsIC ("set" + name)
                                   || m.Name.EqualsIC ("set_" + name));
            return retv.Select(m=> {
                if (m.Name.StartsWith("set_"))
                {
                    return new MethodInfoOrProperty(m, m.DeclaringType.GetProperty(m.Name.Replace("set_",""))); // can prob be optimized
                }
                return new MethodInfoOrProperty(m); 
            });
        }

        public static MethodInfo Match (this IEnumerable<MethodInfo> methods, Type returnType=null, string name=null, IEnumerable<Type> parameters=null)
        {
            var retv = methods.Matches(returnType,name,parameters);
            return retv.FirstOrDefault ();
        }

        public static MethodInfoOrProperty MatchGet (this IEnumerable<MethodInfo> methods, string name, Type returnType=null, IEnumerable<Type> parameters=null)
        {
            var retv = methods.Matches(returnType,null,parameters);
                
            retv = retv.Where (m => m.Name.EqualsIC (name) 
                                    || m.Name.EqualsIC ("get_" + name)
                                    || m.Name.EqualsIC ("get" + name));
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

        public static IEnumerable<MethodInfo> GetOwnPublicMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public| BindingFlags.Instance)
                .Where(m=>!m.DeclaringType.Equals(typeof(Object)))
                .Where(m => !m.Name.StartsWithIC("get_")
                            && !m.Name.StartsWithIC("set_"))
                ;
        }

        public static bool IsClass(this Type t)
        {
            return t.IsClass && t != typeof(String);
        }

        public static bool IsFile(this Type parameterType)
        {
            return parameterType == typeof(FileStream);
        }

        public static bool LooksRequired(this ParameterInfo parameterInfo)
        {
            return !parameterInfo.IsOptional;
        }

        public static IEnumerable<PropertyInfo> GetPublicInstanceProperties(this ParameterInfo parameterInfo)
        {
            return parameterInfo.ParameterType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        public static bool WithName(this MethodInfo m, string action)
        {
            return m.Name.EqualsIC(action);
        }
    }
}
