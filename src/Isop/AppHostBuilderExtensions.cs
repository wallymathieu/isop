using Isop.Abstractions;
using System;

namespace Isop;
/// <summary>
/// Extensions of <see cref="IAppHostBuilder"/>
/// </summary>
public static class AppHostBuilderExtensions
{
    /// <summary>
    /// Recognize <typeparamref name="T"/> as controller
    /// </summary>
    /// <param name="build"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IAppHostBuilder Recognize<T>(this IAppHostBuilder build)
    {
        if (build is null) throw new ArgumentNullException(nameof(build));
        return build.Recognize(typeof(T));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static IAppHostBuilder FormatObjectsAsTable(this IAppHostBuilder build)
    {
        if (build is null) throw new ArgumentNullException(nameof(build));
        return build.SetFormatter(CommandLine.ToStrings.AsTable);
    }
}
