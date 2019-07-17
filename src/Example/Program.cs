using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Isop;

namespace Example
{
    /// <summary>
    /// This is a sample usage of Isop when configuring using <see cref="Builder"/> :
    /// </summary>
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var appHost = Builder
                .Create(new Configuration
                {
                    CultureInfo = CultureInfo.InvariantCulture
                })
                .Recognize(typeof(MyController))
                .Recognize(typeof(CustomerController))
                .BuildAppHost();
            try
            {
                var parsed = appHost.Parse(args);
                if (parsed.Unrecognized.Any()) //Warning:
                {
                    await Console.Error.WriteLineAsync($@"Unrecognized arguments: 
{string.Join(",", parsed.Unrecognized.Select(arg => arg.Value).ToArray())}
Did you mean any of these arguments?
{string.Join(",", parsed.PotentialArguments.Select(a => a.Name).ToArray())}");
                }

                await parsed.InvokeAsync(Console.Out);
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

                return 400;
            }
            catch (MissingArgumentException ex)
            {
                await Console.Error.WriteLineAsync($"Missing argument(s): {String.Join(", ", ex.Arguments).ToArray()}");

                await Console.Error.WriteLineAsync(appHost.Help());
                return 400;
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
                return 500;
            }

            return 0;
        }
    }
}
