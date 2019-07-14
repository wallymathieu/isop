using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Isop.Abstractions;

namespace Isop.Domain
{
    using Infrastructure;
    using CommandLine.Parse;
    using Domain;
    using Microsoft.Extensions.Options;

    internal class ConvertArgumentsToParameterValue
    {
        private readonly TypeConverter _typeConverter;
        private readonly CultureInfo _culture;
        public ConvertArgumentsToParameterValue(IOptions<Configuration> configuration, TypeConverter typeConverter)
        {
            _culture = configuration?.Value.CultureInfo;
            _typeConverter = typeConverter?? throw new ArgumentNullException(nameof(typeConverter));
        }

        public IEnumerable<object> GetParametersForMethod(Method method,
            IEnumerable<KeyValuePair<string,string>> parsedArguments)
        {
            var parameterInfos = method.GetParameters();
            var parameters = new List<object>();

            foreach (var paramInfo in parameterInfos)
            {
                if (paramInfo.IsClassAndNotString() && !paramInfo.IsFile())
                {
                    parameters.Add(CreateObjectFromArguments(parsedArguments, paramInfo));
                }
                else
                {
                    var recognizedArgument = parsedArguments.Where(a => a.Key.EqualsIgnoreCase(paramInfo.Name)).ToArray();
                    parameters.Add(!recognizedArgument.Any()
                        ? paramInfo.DefaultValue
                        : ConvertFrom(recognizedArgument.Single(), paramInfo.ParameterType));
                }
            }
            return parameters;
        }

        private object CreateObjectFromArguments(IEnumerable<KeyValuePair<string,string>> parsedArguments, Parameter paramInfo)
        {
            var obj = Activator.CreateInstance(paramInfo.ParameterType);
            foreach (
                PropertyInfo prop in paramInfo.GetPublicInstanceProperties())
            {
                var recognizedArgument = parsedArguments.First(a => a.Key.EqualsIgnoreCase(prop.Name));
                prop.SetValue(obj, ConvertFrom(recognizedArgument, prop.PropertyType), null);
            }
            return obj;
        }

        private object ConvertFrom(KeyValuePair<string,string> arg1, Type type)
        {
            try
            {
                return _typeConverter(type, arg1.Value, _culture);
            }
            catch (Exception e)
            {
                throw new TypeConversionFailedException("Could not convert argument", e)
                {
                    Argument = arg1.Key,
                    Value = arg1.Value,
                    TargetType = type
                };
            }
        }
    }
}