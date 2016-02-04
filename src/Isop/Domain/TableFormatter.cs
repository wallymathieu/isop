using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
namespace Isop.Domain
{
    class TableFormatter
    {
        public IEnumerable<string> Format(object retval)
        {
            if (retval == null) yield break;
            if (retval is string)
            {
                yield return (retval as string);
            }
            else if (retval.GetType().IsValueType)
            {
                yield return (retval.ToString());
            }
            else if (retval is IEnumerable)
            {
                var if1 = retval.GetType().GetInterfaces()
                    .Single(iff => iff.IsGenericType && iff.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                var type = if1.GetGenericArguments().Single();
                var properties = GetProperties(type);
                yield return Header(properties);
                foreach (var item in (retval as IEnumerable))
                {
                    yield return Line(properties, item);
                }
            }
            else
            {
                var properties = GetProperties(retval.GetType());
                yield return Header(properties);
                yield return Line(properties, retval);
            }
        }

        private static PropertyInfo[] GetProperties(Type t)
        {
            return t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
        }
        private static string Header(PropertyInfo[] properties)
        {
            return string.Join("\t", properties.Select(prop => prop.Name));
        }
        private static string Line(PropertyInfo[] properties, Object item)
        {
            return string.Join("\t", properties.Select(prop => prop.GetValue(item, null)));
        }
    }
}
