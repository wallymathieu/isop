using System;
using System.Globalization;
using System.Linq;
using Isop;

namespace Example
{
    /// <summary>
    /// This is a sample usage of Isop when configuring using <see cref=""/> ArgumentParser.Build:
    /// </summary>
    class Program
    {
        static void Main(string[] args)
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
                if (parsedMethod.UnRecognizedArguments.Any())//Warning:
                {
                    var unRecognizedArgumentsMessage = $@"Unrecognized arguments: 
{string.Join(",", parsedMethod.UnRecognizedArguments.Select(arg => arg.Value).ToArray())}
Did you mean any of these arguments?
{string.Join(",", parsedMethod.ArgumentWithOptions.Select(rec => rec.Name).ToArray())}";
                    Console.WriteLine(unRecognizedArgumentsMessage);
                }else
                {
                    parsedMethod.Invoke(Console.Out);
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
