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
        public ParsedMethod(ParsedArguments parsedArguments, TypeContainer typeContainer, Configuration configuration)
            : base(parsedArguments)
        {
            _typeContainer = typeContainer;
            _configuration = configuration;
        }

        private TypeContainer _typeContainer;
        private Configuration _configuration;

        public Type RecognizedClass { get; set; }
        public Method RecognizedAction { get; set; }

        public IEnumerable<object> RecognizedActionParameters { get; set; }

        public override IEnumerable<string> Invoke()
        {
            var instance = _typeContainer.CreateInstance(RecognizedClass);

            var retval = RecognizedAction.Invoke(instance, RecognizedActionParameters.ToArray());
            return _configuration.Formatter.FormatCommandLine(retval);
        }
    }
}

