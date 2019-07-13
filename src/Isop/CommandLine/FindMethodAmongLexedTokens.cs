using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Isop.Infrastructure;
using Isop.CommandLine.Lex;
using Isop.Domain;
namespace Isop.CommandLine
{
    internal class FindMethodAmongLexedTokens
    {
        public static Method FindMethod(IEnumerable<Method> methods, String methodName, IEnumerable<Token> lexed)
        {
            var potential = methods
                .Where(method => method.Name.EqualsIgnoreCase(methodName));
            var potentialMethod = potential
                .Where(method => method.GetParameters().Length <= lexed.Count(t => t.TokenType == TokenType.Parameter))
                .OrderByDescending(method => method.GetParameters().Length)
                .FirstOrDefault();
            if (potentialMethod != null)
            {
                return potentialMethod;
            }
            return potential.FirstOrDefault();
        }
    }
}
