using System;

namespace Isop.Tests
{
    class EnumerableController
    {
        public Func<object> OnEnumerate;
        public int Length;
        public System.Collections.IEnumerable Return()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return OnEnumerate();
            }
        }
    }
}