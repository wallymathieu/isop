using System;

namespace Tests.FakeControllers
{
    class WithEnumController
    {
        public enum WithEnum
        {
            Param1,
            ParamWithCasing
        }

        public WithEnumController()
        {
            OnIndex = p1 => string.Empty;
        }
        public Func<WithEnum?, string> OnIndex { get; set; }
        public string Index(WithEnum? value = null) { return OnIndex(value); }
    }
}