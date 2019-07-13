using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        private readonly Localization.Texts _helpTexts;

        public HelpForControllers(RecognizesConfiguration recognizes, 
            HelpXmlDocumentation helpXmlDocumentation,
            IOptions<Localization.Texts> helpTexts)
        {
            _classAndMethodRecognizers = recognizes.Recognizes;
            _helpXmlDocumentation = helpXmlDocumentation;
            _helpTexts = helpTexts.Value ?? new Localization.Texts();
        }

        private readonly Type[] _onlyStringType = { typeof(string) };
        public string Description(Controller t, Method method = null, bool includeArguments = false)
        {
            var description = t.Type.GetTypeInfo().GetMethods()
                .SingleOrDefault(m => m.ReturnType == typeof(string)
                && m.Name.EqualsIgnoreCase( Conventions.Help)
                && m.GetParameters().Select(p=>p.ParameterType).SequenceEqual(_onlyStringType));
            var descr = new List<string>();
            if (null == description)
            {
                descr.Add(null == method
                    ? _helpXmlDocumentation.GetDescriptionForType(t.Type)
                    : _helpXmlDocumentation.GetDescriptionForMethod(method.MethodInfo));
            }
            else
            {
                throw new NotImplementedException();
                /*
                var provider = _container.BuildServiceProvider();
                using (var scope = provider.CreateScope())
                {
                    var obj = scope.ServiceProvider.GetService(t.Type);
                    if (ReferenceEquals(null, obj)) throw new Exception($"Unable to resolve {t.Type}");
                    descr.Add((string)description.Invoke(obj, new object[]
                    {
                    method != null ? method.Name : null
                    }));
                }
                */
            }

            if (method != null && includeArguments)
            {
                var arguments = method.GetArguments().Select(DescriptionAndHelp);
                descr.AddRange(arguments);
            }

            if (!descr.Any())
                return string.Empty;
            return "  " + String.Join(" ", descr).Trim();
        }

        private string HelpFor(Controller type, bool simpleDescription)
        {
            if (simpleDescription)
            {
                return type.Name + Description(type);
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
                    _helpTexts.UnknownAction,
                    action
                };
                return string.Join(Environment.NewLine, lines);
            }

            var arguments = method
                .GetArguments()
                .ToArray();
            if (arguments.Any())
            {
                var lines = new[]
                {
                    string.Concat(method.Name, " ",Description(type, method)),
                    string.Concat(_helpTexts.AndAcceptTheFollowingParameters,":"),
                    string.Join(", ", arguments.Select(DescriptionAndHelp)),
                    string.Concat(_helpTexts.AndTheShortFormIs,":"),
                    string.Join(" ", type.Name, method.Name,
                        string.Join(", ", arguments.Select(arg => arg.Name.ToUpperInvariant())))
                };
                return string.Join(Environment.NewLine, lines);
            }
            else
            {
                return string.Concat(method.Name, " ", Description(type, method));
            }
        }

        private string DescriptionAndHelp(Argument argument)
        {
            var options = argument as ArgumentWithOptions;
            if (options != null)
            {
                return options.Argument.Help();
            }
            return "--" + argument.Name;
        }

        public string Help(string val = null, string action = null)
        {
            if (string.IsNullOrEmpty(val))
            {
                var lines = new []
                {
                    _helpTexts.TheCommandsAre,
                    string.Join(Environment.NewLine,
                        _classAndMethodRecognizers
                            .Where(cmr => !cmr.IsHelp())
                            .Select(cmr => "  " + HelpFor(cmr, true)).ToArray()),
                    string.Empty,
                    _helpTexts.HelpCommandForMoreInformation
                };
                return string.Join(Environment.NewLine, lines);
            }
            var controllerRecognizer = _classAndMethodRecognizers.First(type =>
                type.Name.EqualsIgnoreCase(val));
            if (string.IsNullOrEmpty(action))
            {
                return string.Concat(_helpTexts.TheSubCommandsFor,
                       HelpFor(controllerRecognizer, false),
                       Environment.NewLine,
                       Environment.NewLine,
                       _helpTexts.HelpSubCommandForMoreInformation);
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
