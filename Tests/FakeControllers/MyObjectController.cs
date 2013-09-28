using System;

namespace Isop.Tests
{
    class MyObjectController
    {
        public class Argument
        {
            public string param1 { get; set; }
            public string param2 { get; set; }
            public int param3 { get; set; }
            public decimal param4 { get; set; }
        }

        public MyObjectController()
        {
            OnAction = (a) => string.Empty;
        }
        public Func<Argument, string> OnAction { get; set; }
        public string Action(Argument a) { return OnAction(a); }
    }
}