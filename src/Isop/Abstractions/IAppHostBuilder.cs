using System;
using Isop.CommandLine.Parse;

namespace Isop.Abstractions
{
    /// <summary>
    /// App host builder allows you to construct an application host for command line applications.
    /// </summary>
    public interface IAppHostBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        IAppHostBuilder SetTypeConverter(TypeConverter typeConverter);

        /// <summary>
        /// Set how objects are formatted on the command line.
        /// </summary>
        IAppHostBuilder SetFormatter(ToStrings toStrings);

        /// <summary>
        /// Used in order to let your command line tool recognize a parameter. 
        /// You would use this the same way you would an ordinary command line parser (parse a single value).
        /// </summary>
        IAppHostBuilder Parameter(string argument, ArgumentAction? action = null, bool required = false, string? description = null);
        /// <summary>
        /// Used in order to let your command line tool recognize a parameter. 
        /// You would use this the same way you would an ordinary command line parser (parse a single value).
        /// </summary>
        IAppHostBuilder Parameter(ArgumentParameter argument, ArgumentAction? action = null, bool required = false, string? description = null);

        /// <summary>
        /// Used in order to let your command line tool recognize a parameter. 
        /// You would use this the same way you would an ordinary command line parser (parse a single value).
        /// </summary>
        IAppHostBuilder Parameter(string argument, Action<string?> action, bool required = false, string? description = null);
        /// <summary>
        /// Used in order to let your command line tool recognize a parameter. 
        /// You would use this the same way you would an ordinary command line parser (parse a single value).
        /// </summary>
        IAppHostBuilder Parameter(ArgumentParameter argument, Action<string?> action, bool required = false, string? description = null);

        /// <summary>
        /// Tells the builder to recognize the type <paramref name="arg"/> as a controller.
        /// </summary>
        IAppHostBuilder Recognize(Type arg);

        /// <summary>
        /// Build instance of app host.
        /// </summary>
        IAppHost BuildAppHost();
    }
}