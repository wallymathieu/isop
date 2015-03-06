using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using Isop.CommandLine.Parse;
using Isop.Domain;
namespace Isop.CommandLine
{
    public class ParsedMethod : ParsedArguments
    {
        public ParsedMethod(ParsedArguments parsedArguments)
            : base(parsedArguments)
        {
        }
        public Configuration Configuration { get; set; }

        public Type RecognizedClass { get; set; }
        public Method RecognizedAction { get; set; }

        public IEnumerable<object> RecognizedActionParameters { get; set; }

        public override IEnumerable<string> Invoke()
        {
            var instance = Configuration.Factory(RecognizedClass);

            var retval = RecognizedAction.Invoke(instance, RecognizedActionParameters.ToArray());
            return Configuration.Formatter.FormatCommandLine(retval);
        }
    }
}

