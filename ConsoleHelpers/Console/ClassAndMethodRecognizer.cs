using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Helpers.Console
{
    public class ClassAndMethodRecognizer
    {
        public Type Type { get; private set; }
        private readonly Func<Object,Type,Object> _changeType;

        public ClassAndMethodRecognizer(Type type)
            : this(type, Convert.ChangeType)
        {
        }

        public ClassAndMethodRecognizer(Type type, Func<Object,Type,Object> changeType)
        {
            Type = type;
            _changeType = changeType;
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
            var parameterInfos = methodInfo.GetParameters();
            var argumentRecognizers = parameterInfos
                .Select(parameterInfo => new ArgumentRecognizer(parameterInfo.Name)).ToList();
            var parser = new ArgumentParser(argumentRecognizers);
            var parsedArguments = parser.Parse(arg);
            parsedArguments.RecognizedAction = methodInfo;
            parsedArguments.RecognizedActionParameters =
                parsedArguments.RecognizedArguments.Select((arg1, i) => _changeType(arg1.Value, parameterInfos[i].ParameterType));
            return parsedArguments;
        }
    }
}