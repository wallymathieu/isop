using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Isop.Auto.Cli
{
    class Program
    {

        static void Main(string[] args)
        {
            var path =  Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)
                .FullName;
            using(var parserBuilder = new Build().ConfigurationFrom(
                path))
            {
            
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
      String.Join(",", parsedMethod.ArgumentWithOptions.Select(rec => rec.Argument.ToString()).ToArray()));
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
                    Console.WriteLine(String.Format("Missing argument(s): {0}",String.Join(", ",ex.Arguments.Select(a=>String.Format("{0}: {1}",a.Key,a.Value)).ToArray())));
                    if (parserBuilder.RecognizesHelp())
                        Console.WriteLine(parserBuilder.Help());
                }
                catch (NoClassOrMethodFoundException ex)
                {
                    Console.WriteLine("Missing argument(s) or wrong argument(s)");                
                    if (parserBuilder.RecognizesHelp())
                        Console.WriteLine(parserBuilder.Help());
                } 
            }
        }
    }
}
