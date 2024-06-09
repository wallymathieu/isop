using System.Collections;
using System.Linq;

namespace Isop.CommandLine;
internal static class ToStrings
{
    public static IEnumerable<string> Default(object? value)
    {
        if (value == null) yield break;
        switch (value)
        {
            case string s:
                yield return s;
                break;
            case IEnumerable enumerable:
                {
                    foreach (var item in enumerable)
                    {
                        foreach (var formatted in Default(item))
                        {
                            yield return formatted;
                        }
                    }
                    break;
                }
            default:
                yield return value.ToString()!;
                break;
        }
    }

    public static IEnumerable<string> AsTable(object? value)
    {
        if (value == null) yield break;
        switch (value)
        {
            case string s:
                yield return s;
                break;
            case IEnumerable enumerable:
                {
                    var type = GetIEnumerableTypeParameter(enumerable.GetType());
                    if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
                    {
                        foreach (var item in enumerable)
                        {
                            foreach (var formatted in AsTable(item))
                            {
                                yield return formatted;
                            }
                        }
                    }
                    else
                    {
                        var properties = GetProperties(type);
                        yield return Header(properties);
                        foreach (var item in enumerable)
                        {
                            yield return Line(properties, item);
                        }
                    }

                    break;
                }

            default:
                {
                    var properties = GetProperties(value.GetType());
                    yield return Header(properties);
                    yield return Line(properties, value);
                    break;
                }
        }
    }

    private static Type GetIEnumerableTypeParameter(Type iEnumerableType)
    {
        var iEnumerableInterface = iEnumerableType.GetTypeInfo().GetInterfaces()
            .Single(iff => iff.GetTypeInfo().IsGenericType
                           && iff.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return iEnumerableInterface.GetTypeInfo().GetGenericArguments().Single();
    }

    private static PropertyInfo[] GetProperties(Type t)
    {
        return t.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
    }
    private static string Header(PropertyInfo[] properties)
    {
        return string.Join("\t", properties.Select(prop => prop.Name));
    }
    private static string Line(PropertyInfo[] properties, Object item)
    {
        return string.Join("\t", properties.Select(prop => prop.GetValue(item, index: null)));
    }
}
