using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Helpers.Console
{
    public class ClassAndMethodRecognizer
    {
        private readonly CultureInfo _culture;
        public Type Type { get; private set; }
        /// <summary>
        /// </summary>
        public ClassAndMethodRecognizer(Type type, CultureInfo cultureInfo = null)
        {
            Type = type;
            _culture = cultureInfo?? CultureInfo.CurrentCulture;
        }

        public bool Recognize(IEnumerable<string> arg)
        {
            return null != FindMethodInfo(arg);
        }

        private MethodInfo FindMethodInfo(IEnumerable<string> arg)
        {
            var foundClassName = Type.Name.Replace("Controller", "").Equals(arg.ElementAtOrDefault(0), StringComparison.OrdinalIgnoreCase);
            if (foundClassName)
            {
                var methodName = arg.ElementAtOrDefault(1);
                var methodInfo = Type.GetMethods().FirstOrDefault(method => method.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));
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
                .Select(parameterInfo => new ArgumentWithOptions(parameterInfo.Name, required: true)).ToList();

            var parser = new ArgumentParser(argumentRecognizers);

            var parsedArguments = parser.Parse(arg);
            var recognizedActionParameters = from paramInfo in parameterInfos 
                                             join recognizedArgument in parsedArguments.RecognizedArguments on 
                                                paramInfo.Name.ToLowerInvariant()
                                                equals recognizedArgument.Argument.ToLowerInvariant() 
                                             select ConvertFrom(recognizedArgument, paramInfo);

            return new ParsedMethod(parsedArguments)
                       {
                           RecognizedAction = methodInfo,
                           RecognizedActionParameters = recognizedActionParameters,
                           RecognizedClass = Type
                       };
        }

        private object ConvertFrom(RecognizedArgument arg1, ParameterInfo parameterInfo)
        {
            try
            {
                var value = TypeDescriptor.GetConverter(parameterInfo.ParameterType).ConvertFrom(null, _culture, arg1.Value);
                return value;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Could not parse {0} with value: {1}", arg1.WithOptions.Argument, arg1.Value),e);
            }
        }
    }

    public class ParsedMethod : ParsedArguments
    {
        public ParsedMethod(ParsedArguments parsedArguments)
            :base(parsedArguments)
        {
            
        }
        public Type RecognizedClass;
        public MethodInfo RecognizedAction { get; set; }

        public IEnumerable<object> RecognizedActionParameters { get; set; }

        public override void Invoke()
        {
            Invoke(Activator.CreateInstance);
        }

        public void Invoke(Func<Type, object> factory)
        {
            RecognizedAction.Invoke(factory(RecognizedClass), RecognizedActionParameters.ToArray());
        }
    }
}