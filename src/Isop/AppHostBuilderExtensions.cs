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
}