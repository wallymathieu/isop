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
        public static Method FindMethod(IEnumerable<Method> methods, String methodName, IEnumerable<Token> lexed)
        {
            var potential = methods
                .Where(method => method.Name.EqualsIgnoreCase(methodName))
                .ToArray();
            var potentialMethod = potential
                .Where(method => method.GetParameters().Length <= lexed.Count(t => t.TokenType == TokenType.Parameter))
                .OrderByDescending(method => method.GetParameters().Length)
                .FirstOrDefault();
            return potentialMethod ?? potential.FirstOrDefault();
        }
    }
}
