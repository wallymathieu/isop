using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Isop.Help;
using Isop.Infrastructure;
using Isop.Parse;

namespace Isop.Controller
{
    public class HelpForControllers
    {
        private readonly TurnParametersToArgumentWithOptions _turnParametersToArgumentWithOptions;

        private readonly ICollection<Type> _classAndMethodRecognizers;
        private readonly TypeContainer _container;
        private readonly HelpXmlDocumentation _helpXmlDocumentation;
        private string _andAcceptTheFollowingParameters;
        public string TheCommandsAre { get; set; }
        public string TheSubCommandsFor { get; set; }
        public string HelpCommandForMoreInformation { get; set; }

        public string HelpSubCommandForMoreInformation { get; set; }

        public HelpForControllers(ICollection<Type> classAndMethodRecognizers, TypeContainer container, 
            TurnParametersToArgumentWithOptions turnParametersToArgumentWithOptions,
            HelpXmlDocumentation helpXmlDocumentation = null)
        {
            _container = container;
            _turnParametersToArgumentWithOptions = turnParametersToArgumentWithOptions;
            _classAndMethodRecognizers = classAndMethodRecognizers;
            _helpXmlDocumentation = helpXmlDocumentation ?? new HelpXmlDocumentation();
            HelpSubCommandForMoreInformation = "Se 'COMMANDNAME' help <command> <subcommand> for more information";

            HelpCommandForMoreInformation = "Se 'COMMANDNAME' help <command> for more information";
            TheCommandsAre = "The commands are:";
            TheSubCommandsFor = "The sub commands for ";
            _andAcceptTheFollowingParameters = "And accept the following parameters";
        }
        private string Description(Type t, MethodInfo method = null)
        {
            var description = t.GetMethods().Match(returnType: typeof(string),
                                               name: "help",
                                               parameters: new[] { typeof(string) });
            var descr = string.Empty;
            if (null == description)
            {
                if (null == method)
                {
                    descr = _helpXmlDocumentation.GetDescriptionForType(t);
                }
                else
                {
                    descr = _helpXmlDocumentation.GetDescriptionForMethod(method);
                }
            }
            else
            {
                var obj = _container.CreateInstance(t);

                descr = (string)description.Invoke(obj, new[] { (method != null ? method.Name : null) });
            }
            if (string.IsNullOrEmpty(descr))
                return string.Empty;
            return "  " + descr;
        }

        private string HelpFor(Type type, bool simpleDescription)
        {
            if (simpleDescription) return type.ControllerName() + Description(type);

            return type.ControllerName()
                + Environment.NewLine
                + Environment.NewLine
                + String.Join(Environment.NewLine, 
                    type.GetControllerActionMethods()
                        .Select(m => "  " + m.Name + Description(type, m)).ToArray());
        }

        private string HelpForAction(Type type, string action)
        {
            var method = type.GetControllerActionMethods().SingleOrDefault(m => m.WithName(action));
            var arguments = _turnParametersToArgumentWithOptions.GetRecognizers(method).Select(DescriptionAndHelp);
            if (arguments.Any())
            {
                return string.Format(@"{0} {1}
{3}:
{2}", method.Name, Description(type, method), String.Join(", ",
    arguments), _andAcceptTheFollowingParameters);
            }
            else
            {
                return string.Format(@"{0} {1}", method.Name, Description(type, method));
            }
        }

        private string DescriptionAndHelp(ArgumentWithOptions r)
        {
            return r.Help();
        }
        //Description(cmr.Type, methodInfo,
        public string Help(string val = null, string action = null)
        {
            if (string.IsNullOrEmpty(val)) return TheCommandsAre + Environment.NewLine +
                   String.Join(Environment.NewLine,
                               _classAndMethodRecognizers
                               .Where(cmr => cmr != typeof(HelpController))
                               .Select(cmr => "  " + HelpFor(cmr, true)).ToArray())
                   + Environment.NewLine
                   + Environment.NewLine
                   + HelpCommandForMoreInformation;

            var controllerRecognizer = _classAndMethodRecognizers.First(type =>
                type.ControllerName().EqualsIC(val));
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
                ? _classAndMethodRecognizers.Any(cmr => cmr != typeof(HelpController))
                : _classAndMethodRecognizers.Any(cmr => cmr.ControllerName().EqualsIC(val));
        }
    }
}
