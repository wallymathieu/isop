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
        static async Task Main(string[] args)
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
                var parsedMethod = appHost.Parse(args);
                if (parsedMethod.Unrecognized.Any())//Warning:
                {
                    var unRecognizedArgumentsMessage = $@"Unrecognized arguments: 
{string.Join(",", parsedMethod.Unrecognized.Select(arg => arg.Value).ToArray())}
Did you mean any of these arguments?
{string.Join(",", parsedMethod.GlobalArguments.Select(rec => rec.Name).ToArray())}";
                    Console.WriteLine(unRecognizedArgumentsMessage);
                }else
                {
                    await parsedMethod.InvokeAsync(Console.Out);
                }
            }
            catch (TypeConversionFailedException ex)
            {
                
                 Console.WriteLine(
                     $"Could not convert argument {ex.Argument} with value {ex.Value} to type {ex.TargetType}");
                 if (null!=ex.InnerException)
                 {
                     Console.WriteLine("Inner exception: ");
                     Console.WriteLine(ex.InnerException.Message);
                 }
            }
            catch (MissingArgumentException ex)
            {
                Console.WriteLine($"Missing argument(s): {String.Join(", ", ex.Arguments).ToArray()}");
                
                Console.WriteLine(appHost.Help());
            }
        }
    }
}
