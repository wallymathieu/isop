using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using Isop.CommandLine.Parse;
using Isop.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Isop.CommandLine
{
    public class ParsedMethod : ParsedArguments
    {
        public ParsedMethod(ParsedArguments parsedArguments, IServiceCollection typeContainer, Configuration configuration)
            : base(parsedArguments)
        {
            _typeContainer = typeContainer;
            _configuration = configuration;
        }

        private IServiceCollection _typeContainer;
        private Configuration _configuration;

        public Type RecognizedClass { get; set; }
        public Method RecognizedAction { get; set; }

        public IEnumerable<object> RecognizedActionParameters { get; set; }

        public override IEnumerable<string> Invoke()
        {
            var svcProvider = _typeContainer.BuildServiceProvider();
            using (var scope = svcProvider.CreateScope())
            {
                var instance = scope.ServiceProvider.GetService(RecognizedClass);
                if (ReferenceEquals(null, instance)) throw new Exception($"Unable to resolve {RecognizedClass.Name}");
                var retval = RecognizedAction.Invoke(instance, RecognizedActionParameters.ToArray());
                return _configuration.Formatter(retval);
            }
        }
    }
}

