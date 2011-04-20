using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConsoleHelpers
{
    public class ClassAndMethodRecognizer
    {
        public Type Type { get; private set; }

        public ClassAndMethodRecognizer(Type type)
        {
            Type = type;
        }

        public bool Recognize(IEnumerable<string> arg)
        {
            return null != FindMethodInfo(arg);
        }

        private MethodInfo FindMethodInfo(IEnumerable<string> arg)
        {
            var foundClassName = Type.Name.Replace("Controller", "").Equals(arg.ElementAtOrDefault(0), StringComparison.InvariantCultureIgnoreCase);
            if (foundClassName)
            {
                var methodName = arg.ElementAtOrDefault(1);
                var methodInfo = Type.GetMethods().FirstOrDefault(method => method.Name.Equals(methodName, StringComparison.InvariantCultureIgnoreCase));
                return methodInfo;
            }
            return null;
        }

        public ParsedArguments Parse(IEnumerable<string> arg)
        {
            var methodInfo = FindMethodInfo(arg);
            var argumentRecognizers = methodInfo.GetParameters()
                .Select(parameterInfo => new ArgumentRecognizer(parameterInfo.Name)).ToList();
            var parser = new ArgumentParser(argumentRecognizers);
            var parsedArguments = parser.Parse(arg);
            parsedArguments.RecognizedAction = methodInfo;
            parsedArguments.RecognizedActionParameters = parsedArguments.RecognizedArguments.Select(arg1=>(object)arg1.Value);
            return parsedArguments;
        }
    }
}