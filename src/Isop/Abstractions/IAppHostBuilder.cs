using System;

namespace Isop.Abstractions
{
    using CommandLine.Views;
    /// <summary>
    /// 
    /// </summary>
    public interface IAppHostBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        IAppHostBuilder SetTypeConverter(TypeConverter typeConverter);

        /// <summary>
        /// 
        /// </summary>
        IAppHostBuilder SetFormatter(Formatter formatter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="action"></param>
        /// <param name="required"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        IAppHostBuilder Parameter(string argument, ArgumentAction action = null, bool required = false, string description = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="action"></param>
        /// <param name="required"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        IAppHostBuilder Parameter(string argument, Action<string> action, bool required = false, string description = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        IAppHostBuilder Recognize(Type arg);

        /// <summary>
        /// Build instance of app host.
        /// </summary>
        IAppHost BuildAppHost();
    }
}