using System.Collections.Generic;
using System;

namespace Isop.Gui.Models
{
    public class Root
    {
        public IEnumerable<Param> GlobalParameters { get; set; }
        public IEnumerable<Controller> Controllers { get; set; }
    }
}