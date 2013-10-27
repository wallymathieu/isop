using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isop.WpfControls
{
    public class SingleScopeOnly
    {
        private bool ignore = false;
        public void Try(Action action)
        {
            if (ignore) return;
            ignore = true;
            action();
            ignore = false;
        }
    }
}
