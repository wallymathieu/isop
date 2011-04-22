using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers.Console;

namespace Console
{
    /// <summary>
    /// This is a sample usage of console helpers:
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var parsedMethod = ArgumentParser.Build()
                                                      .Recognize(typeof(MyController))
                                                      .Parse( args);
                if (parsedMethod.UnRecognizedArguments.Any())//Warning:
                {
                    var unRecognizedArgumentsMessage = string.Format(
@"Unrecognized arguments: 
{0}
Did you mean any of these arguments?
{1}", String.Join(",", parsedMethod.UnRecognizedArguments.ToArray()),
      String.Join(",", parsedMethod.ArgumentWithOptions.Select(rec => rec.Argument.ToString()).ToArray()));
                    System.Console.WriteLine(unRecognizedArgumentsMessage);
                }
                parsedMethod.Invoke();
            }
            catch (MissingArgumentException ex)
            {
                System.Console.WriteLine("Missing argument(s)");
                if (null != ex.Arguments)
                    System.Console.WriteLine(String.Join(", ", ex.Arguments.Select(arg => arg.ToString())));
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
