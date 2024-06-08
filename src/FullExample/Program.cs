using System;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using Isop;

namespace FullExample
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var appHost = AppHostBuilder
                .Create(new AppHostConfiguration
                {
                    CultureInfo = CultureInfo.GetCultureInfo("sv-SE")
                })
                .Recognize(typeof(CustomerController))
                .BuildAppHost();
            try
            {
                var parsedMethod = appHost.Parse(args);
                if (parsedMethod.Unrecognized.Count != 0)//Warning:
                {
                    Console.Error.WriteLine($@"Unrecognized arguments: 
    {string.Join(",", parsedMethod.Unrecognized.Select(arg => arg.Value).ToArray())}");
                    return 1;
                }else
                {
                    await parsedMethod.InvokeAsync(Console.Out);
                    return 0;
                }
            }
            catch (TypeConversionFailedException ex)
            {
                Console.Error.WriteLine(
                    $"Could not convert argument {ex.Argument} with value {ex.Value} to type {ex.TargetType}");
                if (null!=ex.InnerException)
                {
                    Console.Error.WriteLine("Inner exception: ");
                    Console.Error.WriteLine(ex.InnerException.Message);
                }
                return 9;
            }
            catch (MissingArgumentException ex)
            {
                Console.WriteLine($"Missing argument(s): {string.Join(", ", ex.Arguments).ToArray()}");
                Console.WriteLine(await appHost.HelpAsync());
                return 10;
            }
        }
    }
}
