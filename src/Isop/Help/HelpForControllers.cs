﻿using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Isop.CommandLine;
using Isop.Domain;

namespace Isop.Help;

internal sealed class HelpForControllers(Recognizes recognizes,
    HelpXmlDocumentation helpXmlDocumentation,
    IOptions<Localization.Texts> texts,
    IOptions<AppHostConfiguration> configuration,
    IServiceProvider serviceProvider,
    IOptions<Conventions> conventions)
{
    private readonly IEnumerable<Controller> _classAndMethodRecognizers = recognizes.Controllers;
    private readonly AppHostConfiguration? _configuration = configuration?.Value;
    private readonly Conventions _conventions = conventions.Value ?? throw new ArgumentNullException(nameof(conventions));
    private readonly Localization.Texts _texts = texts.Value ?? new Localization.Texts();
    private readonly Type[] _onlyStringType = { typeof(string) };

    private string Description(Controller t, Method? method, bool includeArguments)
    {
        var description = t.Type.GetTypeInfo().GetMethods()
            .SingleOrDefault(m => m.ReturnType == typeof(string)
            && m.Name.EqualsIgnoreCase(_conventions.Help)
            && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(_onlyStringType));
        var helpText = new List<string>();
        if (null == description)
        {
            helpText.Add(null == method
                ? helpXmlDocumentation.GetDescriptionForType(t.Type)
                : helpXmlDocumentation.GetDescriptionForMethod(method.MethodInfo));
        }
        else
        {
            using var scope = serviceProvider.CreateScope();
            var obj = scope.ServiceProvider.GetService(t.Type) ?? throw new ControllerNotFoundException($"Unable to resolve {t.Type}");
            var text = (string?)description.Invoke(obj, [method?.Name]);
            if (text is not null) helpText.Add(text);
        }

        if (method != null && includeArguments)
        {
            var arguments = method.GetArguments(_configuration?.CultureInfo).Select(DescriptionAndHelp);
            helpText.AddRange(arguments);
        }

        if (helpText.Count == 0)
            return string.Empty;
        return "  " + string.Join(" ", helpText).Trim();
    }

    private string HelpFor(Controller type, bool simpleDescription)
    {
        if (simpleDescription)
        {
            return type.GetName(_conventions) + Description(type, includeArguments: false, method: null);
        }
        return string.Concat(type.GetName(_conventions),
            Environment.NewLine,
            Environment.NewLine,
            string.Join(Environment.NewLine,
                type.GetControllerActionMethods(_conventions)
                    .Select(m => "  " + m.Name + Description(type, m, includeArguments: true)).ToArray()));
    }

    private string HelpForAction(Controller type, string action)
    {
        var method = type.GetMethod(_conventions, action);
        if (method == null)
        {
            var lines = new[]
            {
                    _texts.UnknownAction,
                    action
                };
            return string.Join(Environment.NewLine, lines);
        }

        var arguments = method
            .GetArguments(_configuration?.CultureInfo)
            .ToArray();
        if (arguments.Length != 0)
        {
            var lines = new[]
            {
                    $"{method.Name} {Description(type, method, false)}",
                    $"{_texts.AndAcceptTheFollowingParameters}:",
                    string.Join(", ", arguments.Select(DescriptionAndHelp)),
                    $"{_texts.AndTheShortFormIs}:",
                    string.Join(" ", type.GetName(_conventions), method.Name,
                        string.Join(", ", arguments.Select(arg => arg.Name.ToUpperInvariant())))
                };
            return string.Join(Environment.NewLine, lines);
        }
        else
        {
            return $"{method.Name} {Description(type, method, false)}";
        }
    }

    private string DescriptionAndHelp(Argument argument)
    {
        return argument.Help();
    }

    public string Help(string? val = null, string? action = null)
    {
        if (string.IsNullOrEmpty(val))
        {
            var lines = new[]
            {
                    _texts.TheCommandsAre,
                    string.Join(Environment.NewLine,
                        _classAndMethodRecognizers
                            .Where(cmr => !cmr.IsHelp())
                            .Select(cmr => "  " + HelpFor(cmr, true)).ToArray()),
                    string.Empty,
                    _texts.HelpCommandForMoreInformation
                };
            return string.Join(Environment.NewLine, lines);
        }
        var controllerRecognizer = _classAndMethodRecognizers.First(type =>
            type.GetName(_conventions).EqualsIgnoreCase(val));
        if (string.IsNullOrEmpty(action))
        {
            return string.Concat(_texts.TheSubCommandsFor,
                   HelpFor(controllerRecognizer, false),
                   Environment.NewLine,
                   Environment.NewLine,
                   _texts.HelpSubCommandForMoreInformation);
        }
        return HelpForAction(controllerRecognizer, action!);
    }

    public bool CanHelp(string? val = null)
    {
        return string.IsNullOrEmpty(val)
            ? _classAndMethodRecognizers.Any(cmr => !cmr.IsHelp())
            : _classAndMethodRecognizers.Any(cmr => cmr.GetName(_conventions).EqualsIgnoreCase(val));
    }
}

