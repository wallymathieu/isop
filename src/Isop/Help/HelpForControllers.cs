using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Isop.CommandLine;
using Isop.Help;
using Isop.Infrastructure;
using Isop.CommandLine.Parse;
using Isop.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Isop.Help
{
    internal class HelpForControllers
    {
        private readonly IEnumerable<Controller> _classAndMethodRecognizers;
        private readonly HelpXmlDocumentation _helpXmlDocumentation;
        private readonly Configuration _configration;
        private readonly IServiceProvider _serviceProvider;
        private readonly Localization.Texts _texts;

        public HelpForControllers(Recognizes recognizes, 
            HelpXmlDocumentation helpXmlDocumentation,
            IOptions<Localization.Texts> texts,
            IOptions<Configuration> configration,
            IServiceProvider serviceProvider)
        {
            _classAndMethodRecognizers = recognizes.Controllers;
            _helpXmlDocumentation = helpXmlDocumentation;
            _configration = configration.Value;
            _serviceProvider = serviceProvider;
            _texts = texts.Value ?? new Localization.Texts();
        }

        private readonly Type[] _onlyStringType = { typeof(string) };
        public string Description(Controller t, Method method, bool includeArguments)
        {
            var description = t.Type.GetTypeInfo().GetMethods()
                .SingleOrDefault(m => m.ReturnType == typeof(string)
                && m.Name.EqualsIgnoreCase( Conventions.Help)
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
                var arguments = method.GetArguments(_configration?.CultureInfo).Select(DescriptionAndHelp);
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
                return type.Name + Description(type, includeArguments:false, method: null);
            }
            return string.Concat(type.Name,
                Environment.NewLine,
                Environment.NewLine,
                string.Join(Environment.NewLine,
                    type.GetControllerActionMethods()
                        .Select(m => "  " + m.Name + Description(type, m, includeArguments: true)).ToArray()));
        }

        private string HelpForAction(Controller type, string action)
        {
            var method = type.GetMethod(action);
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
                .GetArguments(_configration?.CultureInfo)
                .ToArray();
            if (arguments.Any())
            {
                var lines = new[]
                {
                    string.Concat(method.Name, " ",Description(type, method, false)),
                    string.Concat(_texts.AndAcceptTheFollowingParameters,":"),
                    string.Join(", ", arguments.Select(DescriptionAndHelp)),
                    string.Concat(_texts.AndTheShortFormIs,":"),
                    string.Join(" ", type.Name, method.Name,
                        string.Join(", ", arguments.Select(arg => arg.Name.ToUpperInvariant())))
                };
                return string.Join(Environment.NewLine, lines);
            }
            else
            {
                return string.Concat(method.Name, " ", Description(type, method, false));
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
                type.Name.EqualsIgnoreCase(val));
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
                : _classAndMethodRecognizers.Any(cmr => cmr.Name.EqualsIgnoreCase(val));
        }
    }
}
