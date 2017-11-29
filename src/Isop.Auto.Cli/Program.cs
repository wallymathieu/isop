using System;
using System.IO;
using System.Linq;
using Isop.CommandLine.Parse;

namespace Isop.Auto.Cli
{
    class Program
    {

        static void Main(string[] args)
        {
            var parserBuilder = new Build().ConfigurationFromAssemblyPath();
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
                }
                else
                {
                    parsedMethod.Invoke(Console.Out);
                }
            }
            catch (TypeConversionFailedException ex)
            {

                Console.WriteLine(String.Format("Could not convert argument {0} with value {1} to type {2}",
                   ex.Argument, ex.Value, ex.TargetType));
                if (null != ex.InnerException)
                {
                    Console.WriteLine("Inner exception: ");
                    Console.WriteLine(ex.InnerException.Message);
                }
            }
            catch (MissingArgumentException ex)
            {
                Console.WriteLine(String.Format("Missing argument(s): {0}", String.Join(", ", ex.Arguments.ToArray())));
                if (parserBuilder.RecognizesHelp)
                    Console.WriteLine(parserBuilder.Help());
            }
#if DEBUG
            catch (Exception ex1)
            {
                Console.WriteLine(string.Join(Environment.NewLine, new object[]{
                        "The invokation failed with exception:",
                        ex1.Message, ex1.StackTrace}));
                return;
            }
#endif
        }

    }
}
