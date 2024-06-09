using System;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;
using Isop;

namespace FullExample;
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
                await Console.Error.WriteLineAsync($@"Unrecognized arguments: 
    {string.Join(",", parsedMethod.Unrecognized.Select(arg => arg.Value).ToArray())}");
                return 1;
            }
            else
            {
                await parsedMethod.InvokeAsync(Console.Out);
                return 0;
            }
        }
        catch (TypeConversionFailedException ex)
        {
            await Console.Error.WriteLineAsync(
                $"Could not convert argument {ex.Argument} with value {ex.Value} to type {ex.TargetType}");
            if (null != ex.InnerException)
            {
                await Console.Error.WriteLineAsync("Inner exception: ");
                await Console.Error.WriteLineAsync(ex.InnerException.Message);
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

