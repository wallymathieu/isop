namespace Isop
{
    using Abstractions;
    using CommandLine;

    /// <summary>
    /// Extensions of <see cref="IAppHostBuilder"/>
    /// </summary>
    public static class AppHostBuilderExtensions
    {
        /// <summary>
        /// Recognize <see cref="T"/> as controller
        /// </summary>
        /// <param name="build"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IAppHostBuilder Recognize<T>(this IAppHostBuilder build)
        {
            return build.Recognize(typeof(T));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IAppHostBuilder FormatObjectsAsTable(this IAppHostBuilder build) => build.SetFormatter(CommandLine.ToStrings.AsTable);
    }
}