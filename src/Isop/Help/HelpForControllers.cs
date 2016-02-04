using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Isop.Help;
using Isop.Infrastructure;
using Isop.CommandLine.Parse;
using Isop.Domain;

namespace Isop.Help
{
    public class HelpForControllers : HelpTexts
    {

        private readonly ICollection<Controller> _classAndMethodRecognizers;
        private readonly TypeContainer _container;
        private readonly HelpXmlDocumentation _helpXmlDocumentation;

        public HelpForControllers(ICollection<Controller> classAndMethodRecognizers, TypeContainer container,
            HelpXmlDocumentation helpXmlDocumentation = null)
        {
            _container = container;
            _classAndMethodRecognizers = classAndMethodRecognizers;
            _helpXmlDocumentation = helpXmlDocumentation ?? new HelpXmlDocumentation();
        }

        private readonly Type[] _onlyStringType = { typeof(string) };
        public string Description(Controller t, Method method = null, bool includeArguments = false)
        {
            var description = t.Type.GetMethods()
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
                var obj = _container.CreateInstance(t.Type);

                descr.Add((string)description.Invoke(obj, new object[]
                {
                    method != null ? method.Name : null
                }));
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
                    UnknownAction,
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
                    string.Concat(AndAcceptTheFollowingParameters,":"),
                    string.Join(", ", arguments.Select(DescriptionAndHelp)),
                    string.Concat(AndTheShortFormIs,":"),
                    string.Join(" ", type.Name, method.Name,
                        string.Join(", ", arguments.Select(arg => arg.Name.ToUpper(CultureInfo.CurrentCulture))))
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
                    TheCommandsAre,
                    string.Join(Environment.NewLine,
                        _classAndMethodRecognizers
                            .Where(cmr => !cmr.IsHelp())
                            .Select(cmr => "  " + HelpFor(cmr, true)).ToArray()),
                    string.Empty,
                    HelpCommandForMoreInformation
                };
                return string.Join(Environment.NewLine, lines);
            }
            var controllerRecognizer = _classAndMethodRecognizers.First(type =>
                type.Name.EqualsIgnoreCase(val));
            if (string.IsNullOrEmpty(action))
            {
                return string.Concat(TheSubCommandsFor,
                       HelpFor(controllerRecognizer, false),
                       Environment.NewLine,
                       Environment.NewLine,
                       HelpSubCommandForMoreInformation);
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
