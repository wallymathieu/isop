using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Options;
using Isop.Abstractions;
using Isop.Domain;

namespace Isop.CommandLine;
internal sealed class ConvertArgumentsToParameterValue(
    IOptions<AppHostConfiguration>? configuration,
    TypeConverter typeConverter)
{
    private readonly TypeConverter _typeConverter = typeConverter ?? throw new ArgumentNullException(nameof(typeConverter));
    private readonly CultureInfo? _culture = configuration?.Value.CultureInfo;

    public bool TryGetParametersForMethod(Method method,
        IReadOnlyCollection<KeyValuePair<string, string?>> parsedArguments,
#if NET8_0_OR_GREATER
            [NotNullWhen(true)]
#endif
        out IReadOnlyCollection<object?>? parameters,
#if NET8_0_OR_GREATER
            [NotNullWhen(false)]
#endif
        out IReadOnlyCollection<string>? missingParameters)
    {
        var parameterInfos = method.GetParameters();
        var parameterValues = new List<object?>();
        var missing = new List<string>();
        foreach (var paramInfo in parameterInfos)
        {
            if (paramInfo.IsClassAndNotString() && !paramInfo.IsFile())
            {
                parameterValues.Add(CreateObjectFromArguments(parsedArguments, paramInfo));
            }
            else
            {
                var recognizedArgument = parsedArguments.Where(a => a.Key.EqualsIgnoreCase(paramInfo.Name)).ToArray();
                if (recognizedArgument.Length == 0 && !paramInfo.HasDefaultValue())
                {
                    missing.Add(paramInfo.Name);
                }
                else
                {
                    parameterValues.Add(recognizedArgument.Length == 0
                        ? paramInfo.DefaultValue
                        : ConvertFrom(recognizedArgument.Single(), paramInfo.ParameterType));
                }
            }
        }

        bool anythingMissing = missing.Count != 0;
        parameters = anythingMissing ? null : parameterValues;
        missingParameters = anythingMissing ? missing : null;
        return !anythingMissing;
    }

    private object CreateObjectFromArguments(IReadOnlyCollection<KeyValuePair<string, string?>> parsedArguments, Parameter paramInfo)
    {
        var obj = Activator.CreateInstance(paramInfo.ParameterType) ?? throw new Exception($"Failed to initialize {paramInfo.ParameterType.Name}");
        foreach (var prop in paramInfo.GetPublicInstanceProperties())
        {
            var recognizedArgument = parsedArguments.FirstOrDefault(a => a.Key.EqualsIgnoreCase(prop.Name));
            if (recognizedArgument.Key != null)
            {
                prop.SetValue(obj, ConvertFrom(recognizedArgument, prop.PropertyType), null);
            }
        }
        return obj;
    }

    private object? ConvertFrom(KeyValuePair<string, string?> arg1, Type type)
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
