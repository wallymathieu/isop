using System;
using System.Collections.Generic;
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

        private string Description(Controller t, Method method = null, bool includeArguments = false)
        {
            var description = t.Type.GetMethods().Where(m=>
                m.ReturnType.Equals( typeof(string))
                && m.Name.EqualsIC( Conventions.Help)
                && m.GetParameters().Select(p=>p.ParameterType).SequenceEqual( new Type[] { typeof(string) }))
                .SingleOrDefault();
            var descr = new List<string>();
            if (null == description)
            {
                if (null == method)
                {
                    descr.Add(_helpXmlDocumentation.GetDescriptionForType(t.Type));
                }
                else
                {
                    descr.Add(_helpXmlDocumentation.GetDescriptionForMethod(method.MethodInfo));
                }
            }
            else
            {
                var obj = _container.CreateInstance(t.Type);

                descr.Add((string)description.Invoke(obj, new[] { (method != null ? method.Name : null) }));
            }

            if (method != null && includeArguments)
            {
                var arguments = method.GetArguments().Select(DescriptionAndHelp);
                descr.AddRange(arguments);
            }

            if (!descr.Any())
                return string.Empty;
            return "  " + String.Join(" ", descr);
        }

        private string HelpFor(Controller type, bool simpleDescription)
        {
            if (simpleDescription)
            {
                return type.Name + Description(type);
            }
            return type.Name
                + Environment.NewLine
                + Environment.NewLine
                + String.Join(Environment.NewLine,
                    type.GetControllerActionMethods()
                        .Select(m => "  " + m.Name + Description(type, m, includeArguments: true)).ToArray());
        }

        private string HelpForAction(Controller type, string action)
        {
            var method = type.GetMethod(action);
            if (method == null)
            {
                var lines = new List<string> 
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
                var lines = new List<string>
                {
                    string.Format("{0} {1}", method.Name, Description(type, method)),
                    string.Format("{0}:", AndAcceptTheFollowingParameters),
                    String.Join(", ", arguments.Select(DescriptionAndHelp)),
                    string.Format("{0}:", AndTheShortFormIs),
                    type.Name + " " + method.Name + " " +
                        String.Join(", ", arguments.Select(arg => arg.Name.ToUpper()))
                };
                return string.Join(Environment.NewLine, lines);
            }
            else
            {
                return string.Format(@"{0} {1}", method.Name, Description(type, method));
            }
        }

        private string DescriptionAndHelp(Argument argument)
        {
            if (argument is ArgumentWithOptions)
            {
                return ((ArgumentWithOptions)argument).Argument.Help();
            }
            return "--"+argument.Name;
        }

        public string Help(string val = null, string action = null)
        {
            if (string.IsNullOrEmpty(val))
            {
                var lines = new List<string>();
                lines.Add(TheCommandsAre);
                lines.Add(String.Join(Environment.NewLine,
                           _classAndMethodRecognizers
                    .Where(cmr => !cmr.IsHelp())
                               .Select(cmr => "  " + HelpFor(cmr, true)).ToArray()));
                lines.Add(string.Empty);
                lines.Add(HelpCommandForMoreInformation);
                return string.Join(Environment.NewLine, lines);
            }
            var controllerRecognizer = _classAndMethodRecognizers.First(type =>
                type.Name.EqualsIC(val));
            if (string.IsNullOrEmpty(action))
            {
                return TheSubCommandsFor +
                       HelpFor(controllerRecognizer, false)
                       + Environment.NewLine
                       + Environment.NewLine
                       + HelpSubCommandForMoreInformation;
            }
            return HelpForAction(controllerRecognizer, action);
        }

        public bool CanHelp(string val = null, string action = null)
        {
            return string.IsNullOrEmpty(val)
                ? _classAndMethodRecognizers.Any(cmr => !cmr.IsHelp())
                : _classAndMethodRecognizers.Any(cmr => cmr.Name.EqualsIC(val));
        }
    }
}
