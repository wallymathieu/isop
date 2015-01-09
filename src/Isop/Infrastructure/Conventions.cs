using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Isop.Infrastructure
{
    public class Conventions
    {
        public Conventions()
        {
            ControllerName = "controller";
            ConfigurationName = "isopconfiguration";
        }

        public string ControllerName{ get; set; }

        public string ConfigurationName { get; set; }
    }
}