﻿using System;
using System.Globalization;
using System.Linq;
using Isop;

namespace Example
{
    /// <summary>
    /// This is a sample usage of Isop when configuring using ArgumentParser.Build:
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var parserBuilder = Build.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(MyController))
            .Recognize(typeof(CustomerController)).Build();
            try
            {
                var parsedMethod = parserBuilder.Parse(args);
                if (parsedMethod.UnRecognizedArguments.Any())//Warning:
                {
                    var unRecognizedArgumentsMessage = string.Format(
@"Unrecognized arguments: 
{0}
Did you mean any of these arguments?
{1}", String.Join(",", parsedMethod.UnRecognizedArguments.Select(unrec => unrec.Value).ToArray()),
      String.Join(",", parsedMethod.ArgumentWithOptions.Select(rec => rec.Name).ToArray()));
                    Console.WriteLine(unRecognizedArgumentsMessage);
                }else
                {
                    parsedMethod.Invoke(Console.Out);
                }
            }
            catch (TypeConversionFailedException ex)
            {
                
                 Console.WriteLine(String.Format("Could not convert argument {0} with value {1} to type {2}", 
                    ex.Argument, ex.Value, ex.TargetType));
                 if (null!=ex.InnerException)
                {
                    Console.WriteLine("Inner exception: ");
                    Console.WriteLine(ex.InnerException.Message);
                }
            }
            catch (MissingArgumentException ex)
            {
                Console.WriteLine(String.Format("Missing argument(s): {0}",String.Join(", ",ex.Arguments).ToArray()));
                
                Console.WriteLine(parserBuilder.Help());
            }
        }
    }
}
