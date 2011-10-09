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
                parsedMethod.Invoke();
            }
            catch (MissingArgumentException ex)
            {
                System.Console.WriteLine("Missing argument(s)");

                System.Console.WriteLine(parserBuilder.Help());
            }
            catch (NoClassOrMethodFoundException ex)
            {
                System.Console.WriteLine("Missing argument(s) or wrong argument(s)");

                System.Console.WriteLine(parserBuilder.Help());
            }
        }
    }

    internal class MyController
    {
        public void Action(string value)
        {
            System.Console.WriteLine("invoking action on mycontroller with value : " + value);
        }
    }
    internal class CustomerController
    {
        public void Add(string name)
        {
            System.Console.WriteLine("invoking action Add on customercontroller with name : " + name);
        }
    }
}
