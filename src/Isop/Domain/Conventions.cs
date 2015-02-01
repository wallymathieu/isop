using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Isop.Domain
{
    public static class Conventions
    {
        static Conventions()
        {
            ControllerName = "controller";
            ConfigurationName = new HashSet<string>(new[] { "isopconfiguration" }, StringComparer.OrdinalIgnoreCase);
            Help = "help";
            Index = "index";
        }

        public static string ControllerName{ get; set; }

        public static HashSet<string> ConfigurationName { get; set; }

        public static string Help{ get; set; }

        public static string Index {get;set;}
    }
}