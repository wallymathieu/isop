using System;
using System.Linq;
namespace Isop.CommandLine
{
    using Infrastructure;
    using Lex;
    using Domain;
    internal static class MethodQueries
    {
        public static Method FindMethod(ILookup<string, Method> methods, String methodName, int parameterCount=0)
        {
            if (!methods.Contains(methodName)) return null;
            var potential = methods[methodName].ToArray();
            var potentialMethod = potential
                .Where(method => method.GetParameters().Length <= parameterCount)
                .OrderByDescending(method => method.GetParameters().Length)
                .FirstOrDefault();
            return potentialMethod ?? potential.FirstOrDefault();
        }
    }
}
