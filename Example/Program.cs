using System;
using System.Linq;
using Isop.Console;

namespace Isop.Example
{
    /// <summary>
    /// This is a sample usage of console helpers:
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var parserBuilder = ArgumentParser.Build()
                       .Recognize(typeof(MyController))
                       .Recognize(typeof(CustomerController));
            if (args.Length==0)
            {
                System.Console.WriteLine(parserBuilder.Help());
                return;
            }
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
                    System.Console.WriteLine(unRecognizedArgumentsMessage);
                }
                System.Console.WriteLine(parsedMethod.Invoke());
            }
            catch (MissingArgumentException)
            {
                System.Console.WriteLine("Missing argument(s)");

                System.Console.WriteLine(parserBuilder.Help());
            }
            catch (NoClassOrMethodFoundException)
            {
                System.Console.WriteLine("Missing argument(s) or wrong argument(s)");

                System.Console.WriteLine(parserBuilder.Help());
            }
        }
    }

    public class MyController
    {
        public void Action(string value)
        {
            System.Console.WriteLine("invoking action on mycontroller with value : " + value);
        }
    }
    public class CustomerController
    {
        public void Add(string name)
        {
            System.Console.WriteLine("invoking action Add on customercontroller with name : " + name);
        }
    }
}
