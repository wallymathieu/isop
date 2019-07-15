using System;
using System.IO;
using System.Runtime.ExceptionServices;

namespace Isop
{
    using Abstractions;
    using CommandLine.Views;

    /// <summary>
    /// Extensions of <see cref="IAppHostBuilder"/>
    /// </summary>
    public static class AppHostBuilderExtensions
    {
        /// <summary>
        /// Add 
        /// </summary>
        /// <param name="build"></param>
        /// <param name="ignoreGlobalUnMatchedParameters"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IAppHostBuilder Recognize<T>(this IAppHostBuilder build, bool ignoreGlobalUnMatchedParameters = false)
        {
            return build.Recognize(typeof(T), ignoreGlobalUnMatchedParameters);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IAppHostBuilder FormatObjectsAsTable(this IAppHostBuilder build) => build.SetFormatter(new TableFormatter().Format);
    }

    public static class T
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