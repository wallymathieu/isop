using System;

namespace Example.Cli
{
    public class MyController
    {
        public string Action(string value)
        {
            return "invoking action on mycontroller with value : " + value;
        }
        public string Fail()
        {
            throw new Exception("Failure!");
        }
        public string ActionWithGlobalParameter(string global)
        {
            return "invoking action with global parameter on mycontroller with value " + global;
        }
        public class Argument
        {
            public string MyProperty { get; set; }

        }
        public string ActionWithObjectArgument(Argument arg) 
        {
            return "Invoking ActionWithObjectArgument " + arg.MyProperty;
        }
    }
}