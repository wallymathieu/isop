using System;

namespace Isop.Tests
{
    internal class MyController
    {
        public MyController()
        {
            OnAction = (p1, p2, p3, p4) => string.Empty;
        }
        public Func<string, string, int, decimal, string> OnAction { get; set; }
        public string Action(string param1, string param2, int param3, decimal param4) { return OnAction(param1, param2, param3, param4); }
    }
}

