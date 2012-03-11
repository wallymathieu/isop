using System;
using System.Collections.Generic;

namespace Example.Cli
{
    /// <summary>
    /// This is a sample usage of Isop when you want to invoke the program using Isop.Cli.exe:
    /// </summary>
    class IsopConfiguration
    {
        public IEnumerable<Type> Recognizes()
        {
            return new[] { typeof(MyController), typeof(CustomerController) };
        }
        public bool RecognizeHelp{get{return true;}}
    }
    
    public class MyController
    {
        public string Action(string value)
        {
            return "invoking action on mycontroller with value : " + value;
        }
    }
    public class CustomerController
    {
        public string Add(string name)
        {
            return "invoking action Add on customercontroller with name : " + name;
        }
        public string Fail()
        {
            throw new Exception("Failure!");
        }
    }
}
