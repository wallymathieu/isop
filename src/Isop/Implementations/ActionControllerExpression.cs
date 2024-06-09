using System.Linq;
using Isop.Abstractions;
using Isop.CommandLine;
using Isop.CommandLine.Parse;

namespace Isop.Implementations;
internal sealed class ActionControllerExpression : IActionOnController
{
    private readonly string _controllerName;
    private readonly AppHost _appHost;
    private readonly ConvertArgumentsToParameterValue _convertArguments;

    public ActionControllerExpression(string controllerName, string actionName, AppHost appHost)
    {
        _controllerName = controllerName;
        Name = actionName;
        _appHost = appHost;
        _convertArguments = new ConvertArgumentsToParameterValue(_appHost.Configuration, _appHost.TypeConverter);
    }

    public IReadOnlyCollection<Argument> Arguments =>
        _appHost.ControllerRecognizer.TryFind(_controllerName, Name, out var controllerAndMethod)
            ? controllerAndMethod.Item2.GetArguments(_appHost.CultureInfo).ToArray()
            : throw new ControllerOrActionNotFoundException($"Could not find method or controller. The controller is named \"{_controllerName}\" and the method is named \"{Name}\".");

    public IParsed Parameters(IReadOnlyCollection<KeyValuePair<string, string?>> parameters)
    {
        var valuePairs = parameters.ToArray();

        var unrecognizedArguments = new List<UnrecognizedArgument>();
        var recognizedArguments = new List<RecognizedArgument>();
        for (int index = 0; index < valuePairs.Length; index++)
        {
            var current = valuePairs[index];
            var prop = _appHost.Recognizes.Properties.FirstOrDefault(p => p.Name.EqualsIgnoreCase(current.Key));
            if (prop != null)
            {
                recognizedArguments.Add(new RecognizedArgument(
                    prop,
                    [index],
                    current.Key,
                    current.Value));
            }
            else
            {
                unrecognizedArguments.Add(new UnrecognizedArgument(index, current.Key));
            }
        }

        var parsedArguments = new ParsedArguments.Properties(
            unrecognized: unrecognizedArguments,
            recognized: recognizedArguments
        );
        if (!_appHost.ControllerRecognizer.TryFind(_controllerName, Name, out var controllerAndMethod))
            return new ParsedExpression(parsedArguments, _appHost);
        var (controller, method) = controllerAndMethod;
        if (!_convertArguments.TryGetParametersForMethod(method, [.. parameters], out var parametersForMethod, out var missingParameters))
            return new ParsedExpression(parsedArguments.Merge(new ParsedArguments.MethodMissingArguments(controller.Type, method, missingParameters!)), _appHost);
        var paramMap = valuePairs
            .Select((value, index) => (value, index))
            .ToDictionary(t => t.value.Key, vp => vp, StringComparer.OrdinalIgnoreCase);
        var recognized = method.GetArguments(_appHost.CultureInfo)
            .SelectMany(arg => arg.Name is not null && paramMap.TryGetValue(arg.Name, out var value)
                ? [(arg, value)]
                : Array.Empty<(Argument, (KeyValuePair<string, string?>, int))>())
            .Select(chosen =>
            {
                var (arg, value) = chosen;
                return new RecognizedArgument(arg, [value.Item2], value.Item1.Key, value.Item1.Value);
            }).ToArray();
        return new ParsedExpression(
            parsedArguments.Merge(new ParsedArguments.Method(
                recognizedActionParameters: parametersForMethod!,
                recognized:
                    recognized,
                recognizedClass: controller.Type,
                recognizedAction: method)),
            _appHost);
    }
    public string Help() =>
        (_appHost.HelpController.Index(_controllerName, Name) ?? string.Empty).Trim();

    public string Name { get; }
}
