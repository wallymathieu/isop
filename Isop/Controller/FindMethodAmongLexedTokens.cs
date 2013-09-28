using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Isop.Lex;

namespace Isop.Controller
{
    public class FindMethodAmongLexedTokens
    {
        public static MethodInfo FindMethod(IEnumerable<MethodInfo> methods, String methodName, IEnumerable<Token> lexed)
        {
            var potential = methods
                .Where(method => method.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));
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
