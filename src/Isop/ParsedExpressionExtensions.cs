using System;
using System.IO;
using System.Runtime.ExceptionServices;
using Isop.Abstractions;

namespace Isop
{
    public static class ParsedExpressionExtensions
    {
        
        /// <summary>
        /// 
        /// </summary>
        public static void Invoke(this IParsed parsed, TextWriter output)
        {
            parsed.InvokeAsync(output).GetAwaiter().GetResult();
        }
    }
}