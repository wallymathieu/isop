using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isop.Gui
{
    public class SingleScopeOnly
    {
        private bool ignore = false;
        [Obsolete("This construct should not be used")]
        public void Try(Action action)
        {
            if (ignore) return;
            ignore = true;
            action();
            ignore = false;
        }
    }
}
