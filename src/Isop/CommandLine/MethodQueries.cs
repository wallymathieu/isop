using System.Linq;
using Isop.Domain;

namespace Isop.CommandLine;

internal static class MethodQueries
{
    public static Method? FindMethod(ILookup<string, Method> methods, string methodName, int parameterCount = 0)
    {
        if (!methods.Contains(methodName)) return null;
        var potential = methods[methodName].ToArray();
        var potentialMethod = potential
            .Where(method => method.GetParameters().Count <= parameterCount)
            .OrderByDescending(method => method.GetParameters().Count)
            .FirstOrDefault();
        return potentialMethod ?? potential.FirstOrDefault();
    }
}
