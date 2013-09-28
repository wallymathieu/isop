using System;

namespace Isop.Tests
{
    class MyOptionalController
    {
        public MyOptionalController()
        {
            OnAction = (p1, p2, p3, p4) => string.Empty;
        }
        public Func<string, string, int?, decimal, string> OnAction { get; set; }
        public string Action(string param1, string param2 = null, int? param3 = null, decimal param4 = 1) { return OnAction(param1, param2, param3, param4); }
    }
}