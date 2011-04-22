using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers.Console;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var arguments = ArgumentParser.Build()
                           .Recognize(typeof(MyController))
                           .ParseMethod(args);
                if (arguments.Arguments.UnRecognizedArguments.Any())//Warning:
                    System.Console.WriteLine(arguments.Arguments.UnRecognizedArgumentsMessage());

                arguments.Invoke();
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
