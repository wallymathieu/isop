using System;
using System.Collections.Generic;

namespace Example.Cli
{
    /// <summary>
    /// This is a sample usage of Isop when you want to invoke the program using Isop.Cli.exe:
    /// </summary>
    class IsopConfiguration
    {
        public IEnumerable<Type> Recognizes()
        {
            return new[] { typeof(MyController), typeof(CustomerController) };
        }
        public string Global { get; set; }
        public bool RecognizeHelp{get{return true;}}
    }
}