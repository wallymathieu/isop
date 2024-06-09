using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Options;
using Isop.CommandLine.Lex;
using Isop.CommandLine.Parse;
using Isop.Domain;
using Isop.Abstractions;

namespace Isop.CommandLine;
internal sealed class ControllerRecognizer
{
    private readonly bool _allowInferParameter;
    private readonly IOptions<AppHostConfiguration>? _configuration;
    private readonly Conventions _conventions;
    private readonly ConvertArgumentsToParameterValue _convertArgument;
    /// <summary>
    /// controller name -> controller, action name -> action
    /// </summary>
    private readonly Dictionary<string, (Controller, ILookup<string, Method>)> _controllerActionMap;

    public ControllerRecognizer(
        IOptions<AppHostConfiguration>? configuration,
        TypeConverter typeConverterFunc,
        IOptions<Conventions> conventions,
        Recognizes recognizes)
    {
        _configuration = configuration;
        _conventions = conventions.Value ?? throw new ArgumentNullException(nameof(conventions));
        _allowInferParameter = !(_configuration?.Value?.DisableAllowInferParameter ?? false);
        _convertArgument = new ConvertArgumentsToParameterValue(configuration, typeConverterFunc);
        _controllerActionMap = recognizes.Controllers
            .ToDictionary(
                c => c.GetName(_conventions),
                c => (c, c.GetControllerActionMethods(_conventions)
                                .ToLookup(m => m.Name, m => m, StringComparer.OrdinalIgnoreCase)),
                StringComparer.OrdinalIgnoreCase);
    }

    private CultureInfo? Culture => _configuration?.Value?.CultureInfo;

    public bool TryRecognize(IEnumerable<string> arg, out (Controller, Method) controllerAndMethod)
    {
        var lexed = RewriteLexedTokensToSupportHelpAndIndex.Rewrite(_conventions, ArgumentLexer.Lex(arg).ToList());
        if (_controllerActionMap.TryGetValue(lexed.ElementAtOrDefault(0).Value,
            out var controllerAndMap))
        {
            var (controller, methodMap) = controllerAndMap;
            var method = MethodQueries.FindMethod(methodMap, lexed.ElementAtOrDefault(1).Value, lexed.Count(t => t.TokenType == TokenType.Parameter));
            if (method != null)
            {
                controllerAndMethod = (controller, method);
                return true;
            }
        }
        controllerAndMethod = default;
        return false;
    }

    public bool TryFind(string controllerName, string actionName,
        out (Controller, Method) controllerAndMethod)
    {
        if (_controllerActionMap.TryGetValue(controllerName,
            out var controllerAndMap))
        {
            var (controller, methodMap) = controllerAndMap;
            var method = MethodQueries.FindMethod(methodMap, actionName);
            if (method != null)
            {
                controllerAndMethod = (controller, method);
                return true;
            }
        }
        controllerAndMethod = default;
        return false;
    }

    public ParsedArguments Parse(Controller controller, Method method, IReadOnlyCollection<string> arg)
    {
        var argumentRecognizers = method.GetArguments(Culture)
            .ToList();
        argumentRecognizers.InsertRange(0, new[] {
                new Argument(parameter: ArgumentParameter.Parse("#0" + controller.GetName(_conventions), Culture), required: true),
                new Argument(parameter: ArgumentParameter.Parse("#1" + method.Name, Culture), required: false)
            });
        var controllerLexed = RewriteLexedTokensToSupportHelpAndIndex.Rewrite(_conventions, ArgumentLexer.Lex(arg).ToList());
        var parser = new ArgumentParser(argumentRecognizers, _allowInferParameter);
        var parsedArguments = parser.Parse(controllerLexed, arg);

        if (!_convertArgument.TryGetParametersForMethod(method,
            parsedArguments.Recognized
                .Select(a => new KeyValuePair<string, string?>(a.RawArgument, a.Value))
                .ToArray(), out var recognizedActionParameters, out var missingParameters))
            return new ParsedArguments.MethodMissingArguments(
                missingParameters: missingParameters!,
                recognizedClass: controller.Type,
                recognizedAction: method);
        return new ParsedArguments.Method(
            recognizedActionParameters: recognizedActionParameters!,
            recognized: parsedArguments.Recognized,
            recognizedClass: controller.Type,
            recognizedAction: method);
    }

    public bool TryFindController(string name,
#if NET8_0_OR_GREATER
            [NotNullWhen(true)]
#endif
        out Controller? controller)
    {
        if (_controllerActionMap.TryGetValue(name,
            out var controllerAndMap))
        {
            var (foundController, _) = controllerAndMap;
            controller = foundController;
            return true;
        }
        controller = default;
        return false;
    }
}
