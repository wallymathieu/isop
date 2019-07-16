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
        public static void Invoke(this IParsedExpression parsedExpression, TextWriter output)
        {
            try
            {
                parsedExpression.InvokeAsync(output).Wait();
            }
            catch (AggregateException e)
            {
                if (e.InnerException!=null && e.InnerExceptions.Count==1)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                }
                throw;
            }
        }
    }
}