using System;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Linq;
namespace Isop
{
    public class MethodInfoOrProperty
    {
        public PropertyInfo Property { get; private set; }

        public string Name
        {
            get { return MethodInfo.Name; }
        }

        public MethodInfoOrProperty(MethodInfo methodInfo, PropertyInfo property=null)
        {
            MethodInfo = methodInfo;
            Property = property;
        }

        public MethodInfo MethodInfo { get; private set; }

        public object Invoke(object instance, object[] parameters)
        {
             return MethodInfo.Invoke(instance,parameters);
        }

        public bool Required
        {
            get
            {
                if (null != Property && Property.GetCustomAttributes(typeof(RequiredAttribute), true).Any())
                    return true;
                if (null != MethodInfo && MethodInfo.GetCustomAttributes(typeof(RequiredAttribute), true).Any())
                    return true;
                return false;
            }
        }
    }
}

