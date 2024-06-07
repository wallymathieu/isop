using System;
using System.Collections.Generic;
using System.Threading;
namespace Example
{
    public class MyController
    {
        public string Action(string value)
        {
            return "invoking action on mycontroller with value : " + value;
        }
        public string ActionWithInt(int value)
        {
            return "invoking action with int on mycontroller with value : " + value;
        }
        public IEnumerable<string> ActionYields10Responses(string value)
        {
            yield return "invoking action on mycontroller with value : " + value;
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                yield return "i:" + i;
            }
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
            public string? MyProperty { get; set; }

        }
        public string ActionWithObjectArgument(Argument arg)
        {
            return "Invoking ActionWithObjectArgument " + arg.MyProperty;
        }
    }
}