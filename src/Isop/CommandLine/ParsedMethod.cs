using System;
using System.Collections.Generic;
using System.Linq;
using Isop.CommandLine.Parse;
using Isop.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Isop.CommandLine
{
    public class ParsedMethod : ParsedArguments
    {
        public ParsedMethod(ParsedArguments parsedArguments,
            IServiceProvider typeContainer,
            Formatter formatter,
            Type recognizedClass,
            Method recognizedAction,
            IEnumerable<object> recognizedActionParameters)
            : base(parsedArguments)
        {
            _typeContainer = typeContainer;
            _formatter = formatter;
            RecognizedClass = recognizedClass;
            RecognizedAction = recognizedAction;
            RecognizedActionParameters = recognizedActionParameters;
        }

        private IServiceProvider _typeContainer;
        private readonly Formatter _formatter;
        public Type RecognizedClass { get; private set; }
        public Method RecognizedAction { get; private set; }

        public IEnumerable<object> RecognizedActionParameters { get; private set; }

        public override IEnumerable<string> Invoke()
        {
            using (var scope = _typeContainer.CreateScope())
            {
                var instance = scope.ServiceProvider.GetService(RecognizedClass);
                if (ReferenceEquals(null, instance)) throw new Exception($"Unable to resolve {RecognizedClass.Name}");
                var retval = RecognizedAction.Invoke(instance, RecognizedActionParameters.ToArray());
                return _formatter(retval);
            }
        }
    }
}

