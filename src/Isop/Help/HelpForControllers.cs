using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Isop.Help
{
    using CommandLine;
    using Infrastructure;
    using Domain;
    internal class HelpForControllers
    {
        private readonly IEnumerable<Controller> _classAndMethodRecognizers;
        private readonly HelpXmlDocumentation _helpXmlDocumentation;
        private readonly Configuration _configuration;
        private readonly Conventions _conventions;
        private readonly IServiceProvider _serviceProvider;
        private readonly Localization.Texts _texts;

        public HelpForControllers(Recognizes recognizes, 
            HelpXmlDocumentation helpXmlDocumentation,
            IOptions<Localization.Texts> texts,
            IOptions<Configuration> configuration,
            IServiceProvider serviceProvider,
            IOptions<Conventions> conventions)
        {
            _classAndMethodRecognizers = recognizes.Controllers;
            _helpXmlDocumentation = helpXmlDocumentation;
            _configuration = configuration?.Value;
            _serviceProvider = serviceProvider;
            _conventions = conventions.Value ?? throw new ArgumentNullException(nameof(conventions));
            _texts = texts.Value ?? new Localization.Texts();
        }

        private readonly Type[] _onlyStringType = { typeof(string) };

        private string Description(Controller t, Method method, bool includeArguments)
        {
            var description = t.Type.GetTypeInfo().GetMethods()
                .SingleOrDefault(m => m.ReturnType == typeof(string)
                && m.Name.EqualsIgnoreCase( _conventions.Help)
                && m.GetParameters().Select(p=>p.ParameterType).SequenceEqual(_onlyStringType));
            var helpText = new List<string>();
            if (null == description)
            {
                helpText.Add(null == method
                    ? _helpXmlDocumentation.GetDescriptionForType(t.Type)
                    : _helpXmlDocumentation.GetDescriptionForMethod(method.MethodInfo));
            }
            else
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var obj = scope.ServiceProvider.GetService(t.Type);
                    if (ReferenceEquals(null, obj)) throw new Exception($"Unable to resolve {t.Type}");
                    helpText.Add((string)description.Invoke(obj, new object[]{method?.Name}));
                }
            }

            if (method != null && includeArguments)
            {
                var arguments = method.GetArguments(_configuration?.CultureInfo).Select(DescriptionAndHelp);
                helpText.AddRange(arguments);
            }

            if (!helpText.Any())
                return string.Empty;
            return "  " + String.Join(" ", helpText).Trim();
        }

        private string HelpFor(Controller type, bool simpleDescription)
        {
            if (simpleDescription)
            {
                return type.GetName(_conventions) + Description(type, includeArguments:false, method: null);
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
                var lines = new []
                {
                    _texts.UnknownAction,
                    action
                };
                return string.Join(Environment.NewLine, lines);
            }

            var arguments = method
                .GetArguments(_configuration?.CultureInfo)
                .ToArray();
            if (arguments.Any())
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

        public string Help(string val = null, string action = null)
        {
            if (string.IsNullOrEmpty(val))
            {
                var lines = new []
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
            return HelpForAction(controllerRecognizer, action);
        }

        public bool CanHelp(string val = null)
        {
            return string.IsNullOrEmpty(val)
                ? _classAndMethodRecognizers.Any(cmr => !cmr.IsHelp())
                : _classAndMethodRecognizers.Any(cmr => cmr.GetName(_conventions).EqualsIgnoreCase(val));
        }
    }
}
