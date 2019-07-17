using System;
using System.Collections.Generic;
using System.Linq;
namespace Isop.CommandLine
{
    using Infrastructure;
    using Lex;
    using Domain;
    internal static class FindMethodAmongLexedTokens
    {
        public static Method FindMethod(ILookup<string, Method> methods, String methodName, IEnumerable<Token> lexed)
        {
            if (!methods.Contains(methodName)) return null;
            var potential = methods[methodName].ToArray();
            var potentialMethod = potential
                .Where(method => method.GetParameters().Length <= lexed.Count(t => t.TokenType == TokenType.Parameter))
                .OrderByDescending(method => method.GetParameters().Length)
                .FirstOrDefault();
            return potentialMethod ?? potential.FirstOrDefault();
        }
    }
}
