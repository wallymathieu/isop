using System.Globalization;
using System.Threading.Tasks;
using Isop;

namespace Example
{
    /// <summary>
    /// This is a sample usage of Isop when configuring using <see cref="AppHostBuilder"/> :
    /// </summary>
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var appHost = AppHostBuilder
                .Create(new AppHostConfiguration
                {
                    CultureInfo = CultureInfo.InvariantCulture
                })
                .Recognize(typeof(MyController))
                .Recognize(typeof(CustomerController))
                .BuildAppHost();
            return await appHost.Parse(args).TryInvokeAsync();
        }
    }
}
