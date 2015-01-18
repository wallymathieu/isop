using System.Reflection;
using Isop.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Isop.Infrastructure
{
    public class MethodInfoOrProperty
    {
        public PropertyInfo Property { get; private set; }

        public string Name
        {
            get { return MethodInfo.Name; }
        }

        private static readonly Regex _getOrSet = new Regex("^(get|set)_", RegexOptions.IgnoreCase);

        public MethodInfoOrProperty(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            if (_getOrSet.IsMatch(methodInfo.Name))
            {
                Property = methodInfo.DeclaringType.GetProperty(_getOrSet.Replace(methodInfo.Name, ""));
            }
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
                if (null != Property && IsRequired(Property))
                    return true;
                if (null != MethodInfo && IsRequired(MethodInfo))
                    return true;
                return false;
            }
        }

        private static bool IsRequired(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), true).Any();
        }

        private static bool IsRequired(MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttributes(typeof(RequiredAttribute), true).Any();
        }
    }

}

