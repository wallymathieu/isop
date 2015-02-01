using System.Collections.Generic;
using System;

namespace Isop.Server.Models
{
    public class MethodTreeModel
    {
        public MethodTreeModel()
        {
        }
        public MethodTreeModel(IEnumerable<Param> globalParameters, IEnumerable<Controller> controllers)
        {
            GlobalParameters=globalParameters;
            Controllers=controllers;
        }
        public IEnumerable<Param> GlobalParameters { get; set; }
        public IEnumerable<Controller> Controllers { get; set; }
    }
}