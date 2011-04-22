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
        /// <summary>
        /// </summary>
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
        /// <summary>
        /// Note that in order to register a converter you can use:
        /// TypeDescriptor.AddAttributes(typeof(AType), new TypeConverterAttribute(typeof(ATypeConverter)));
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public ParsedMethod Parse(IEnumerable<string> arg)
        {
            var methodInfo = FindMethodInfo(arg);
            var parameterInfos = methodInfo.GetParameters();
            var argumentRecognizers = parameterInfos
                .Select(parameterInfo => new ArgumentRecognizer(parameterInfo.Name, required: true)).ToList();

            var parser = new ArgumentParser(argumentRecognizers);

            var parsedArguments = parser.Parse(arg);
            return new ParsedMethod
                       {
                           Arguments = parsedArguments,
                           RecognizedAction = methodInfo,
                           RecognizedActionParameters = parsedArguments.RecognizedArguments
                               .Select(
                                   (arg1, i) => TypeDescriptor.GetConverter(parameterInfos[i].ParameterType).ConvertFrom(arg1.Value))
                               .ToList(),
                           RecognizedClass = Type
                       };
        }
    }

    public class ParsedMethod
    {
        public Type RecognizedClass;
        public MethodInfo RecognizedAction { get; set; }

        public IEnumerable<object> RecognizedActionParameters { get; set; }

        public ParsedArguments Arguments { get; set; }

        public void Invoke()
        {
            Invoke(Activator.CreateInstance);
        }

        public void Invoke(Func<Type, object> factory)
        {
            RecognizedAction.Invoke(factory(RecognizedClass), RecognizedActionParameters.ToArray());
        }
    }
}