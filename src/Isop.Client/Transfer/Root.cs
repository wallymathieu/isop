using System.Collections.Generic;
using System;

namespace Isop.Client.Transfer
{
    public class Root
    {
        public IEnumerable<Param> GlobalParameters { get; set; }
        public IEnumerable<Controller> Controllers { get; set; }
    }
}