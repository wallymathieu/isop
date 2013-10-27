using System;

namespace Isop.Tests.FakeControllers
{
    class WithIndexController
    {
        public WithIndexController()
        {
            OnIndex = (p1, p2, p3, p4) => string.Empty;
        }
        public Func<string, string, int, decimal, string> OnIndex { get; set; }
        public string Index(string param1, string param2, int param3, decimal param4) { return OnIndex(param1, param2, param3, param4); }
    }
}