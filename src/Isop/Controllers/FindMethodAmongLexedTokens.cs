using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Isop.Infrastructure;
using Isop.Lex;

namespace Isop.Controllers
{
    public class FindMethodAmongLexedTokens
    {
        public static MethodInfo FindMethod(IEnumerable<MethodInfo> methods, String methodName, IEnumerable<Token> lexed)
        {
            var potential = methods
                .Where(method => method.Name.EqualsIC(methodName));
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
