using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Isop.Infrastructure;
using Isop.Parse;

namespace Isop.Controller
{
    public class TurnParametersToArgumentWithOptions
    {
        private readonly CultureInfo _culture;
        private readonly Func<Type, string, CultureInfo, object> _typeConverter;
        public TurnParametersToArgumentWithOptions(CultureInfo culture, Func<Type, string, CultureInfo, object> typeConverter)
        {
            _culture = culture;
            _typeConverter = typeConverter;
        }

        public IEnumerable<ArgumentWithOptions> GetRecognizers(MethodBase method)
        {
            var parameterInfos = method.GetParameters();
            var recognizers = new List<ArgumentWithOptions>();
            foreach (var parameterInfo in parameterInfos)
            {
                if (parameterInfo.ParameterType.IsClass() && !parameterInfo.ParameterType.IsFile())
                {
                    AddArgumentWithOptionsForPropertiesOnObject(recognizers, parameterInfo);
                }
                else
                {
                    var arg = GetArgumentWithOptions(parameterInfo);
                    recognizers.Add(arg);
                }
            }
            return recognizers;
        }

        private ArgumentWithOptions GetArgumentWithOptions(ParameterInfo parameterInfo)
        {
            return new ArgumentWithOptions(ArgumentParameter.Parse(parameterInfo.Name, _culture),
                                           required: parameterInfo.LooksRequired(),
                                           type: parameterInfo.ParameterType);
        }

        private void AddArgumentWithOptionsForPropertiesOnObject(List<ArgumentWithOptions> recognizers, ParameterInfo parameterInfo)
        {
            recognizers.AddRange(parameterInfo.GetPublicInstanceProperties()
                .Select(prop => 
                    new ArgumentWithOptions(ArgumentParameter.Parse(prop.Name, _culture), 
                        required: parameterInfo.LooksRequired() && prop.Required(), 
                        type: prop.PropertyType)));
        }


        public IEnumerable<object> GetParametersForMethod(MethodInfo method,
                                                          ParsedArguments parsedArguments)
        {
            var parameterInfos = method.GetParameters();
            var parameters = new List<object>();

            foreach (var paramInfo in parameterInfos)
            {
                if (paramInfo.ParameterType.IsClass() && !paramInfo.ParameterType.IsFile())
                {
                    parameters.Add(CreateObjectFromArguments(parsedArguments, paramInfo));
                }
                else
                {
                    var recognizedArgument = parsedArguments.RecognizedArguments.FirstOrDefault(a => a.Matches(paramInfo));
                    parameters.Add(null == recognizedArgument
                                       ? paramInfo.DefaultValue
                                       : ConvertFrom(recognizedArgument, paramInfo.ParameterType));
                }
            }
            return parameters;
        }

        private object CreateObjectFromArguments(ParsedArguments parsedArguments, ParameterInfo paramInfo)
        {
            var obj = Activator.CreateInstance(paramInfo.ParameterType);
            foreach (
                PropertyInfo prop in
                    paramInfo.ParameterType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var recognizedArgument = parsedArguments.RecognizedArguments.First(a => a.Matches(prop));
                prop.SetValue(obj, ConvertFrom(recognizedArgument, prop.PropertyType), null);
            }
            return obj;
        }

        private object ConvertFrom(RecognizedArgument arg1, Type type)
        {
            try
            {
                var typeConverter = _typeConverter ?? DefaultConvertFrom;
                return typeConverter(type, arg1.Value, _culture);
            }
            catch (Exception e)
            {
                throw new TypeConversionFailedException("Could not convert argument", e)
                          {
                              Argument = arg1.WithOptions.Argument.ToString(),
                              Value = arg1.Value,
                              TargetType = type
                          };
            }
        }
        private static object DefaultConvertFrom(Type type, string s, CultureInfo cultureInfo)
        {
            if (type == typeof(FileStream))
            {
                return new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            return TypeDescriptor.GetConverter(type).ConvertFrom(null, cultureInfo, s);
        }

    }
}