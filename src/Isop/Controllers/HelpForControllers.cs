using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Isop.Help;
using Isop.Infrastructure;
using Isop.Parse;

namespace Isop.Controllers
{
    public class HelpTexts
    {
        public HelpTexts()
        {
            HelpSubCommandForMoreInformation = "Se 'COMMANDNAME' help <command> <subcommand> for more information";

            HelpCommandForMoreInformation = "Se 'COMMANDNAME' help <command> for more information";
            TheCommandsAre = "The commands are:";
            TheSubCommandsFor = "The sub commands for ";
            AndAcceptTheFollowingParameters = "And accept the following parameters";
            AndTheShortFormIs = "And the short form is";
            UnknownAction = "Unknown action";
        }

        /// <summary>
        /// default: And accept the following parameters
        /// </summary>
        public string AndAcceptTheFollowingParameters { get; set; }
        /// <summary>
        /// default: And the short form is
        /// </summary>
        public string AndTheShortFormIs { get; set; }
        /// <summary>
        /// default: "The commands are:"
        /// </summary>
        public string TheCommandsAre { get; set; }
        /// <summary>
        /// default: The sub commands for 
        /// </summary>
        public string TheSubCommandsFor { get; set; }
        /// <summary>
        /// default: "Se 'COMMANDNAME' help command for more information"
        /// </summary>
        public string HelpCommandForMoreInformation { get; set; }
        /// <summary>
        /// default: Se 'COMMANDNAME' help 'command' 'subcommand' for more information
        /// </summary>
        public string HelpSubCommandForMoreInformation { get; set; }
        public string UnknownAction { get; set; }
    }

    public class HelpForControllers : HelpTexts
    {
        private readonly TurnParametersToArgumentWithOptions _turnParametersToArgumentWithOptions;

        private readonly ICollection<Type> _classAndMethodRecognizers;
        private readonly TypeContainer _container;
        private readonly HelpXmlDocumentation _helpXmlDocumentation;

        public HelpForControllers(ICollection<Type> classAndMethodRecognizers, TypeContainer container,
            TurnParametersToArgumentWithOptions turnParametersToArgumentWithOptions,
            HelpXmlDocumentation helpXmlDocumentation = null)
        {
            _container = container;
            _turnParametersToArgumentWithOptions = turnParametersToArgumentWithOptions;
            _classAndMethodRecognizers = classAndMethodRecognizers;
            _helpXmlDocumentation = helpXmlDocumentation ?? new HelpXmlDocumentation();
        }

        private string Description(Type t, MethodInfo method = null, bool includeArguments = false)
        {
            var description = t.GetMethods().Match(returnType: typeof(string),
                                               name: "help",
                                               parameters: new[] { typeof(string) });
            var descr = new List<string>();
            if (null == description)
            {
                if (null == method)
                {
                    descr.Add(_helpXmlDocumentation.GetDescriptionForType(t));
                }
                else
                {
                    descr.Add(_helpXmlDocumentation.GetDescriptionForMethod(method));
                }
            }
            else
            {
                var obj = _container.CreateInstance(t);

                descr.Add((string)description.Invoke(obj, new[] { (method != null ? method.Name : null) }));
            }

            if (method != null && includeArguments)
            {
                var arguments = _turnParametersToArgumentWithOptions.GetRecognizers(method).Select(DescriptionAndHelp);
                descr.AddRange(arguments);
            }

            if (!descr.Any())
                return string.Empty;
            return "  " + String.Join(" ", descr);
        }

        private string HelpFor(Type type, bool simpleDescription)
        {
            if (simpleDescription)
            {
                return type.ControllerName() + Description(type);
            }
            return type.ControllerName()
                + Environment.NewLine
                + Environment.NewLine
                + String.Join(Environment.NewLine,
                    type.GetControllerActionMethods()
                        .Select(m => "  " + m.Name + Description(type, m, includeArguments: true)).ToArray());
        }

        private string HelpForAction(Type type, string action)
        {
            var method = type.GetControllerActionMethods().SingleOrDefault(m => m.WithName(action));
            if (method == null)
            {
                var lines = new List<string> 
                {
                    UnknownAction,
                    action
                };
                return string.Join(Environment.NewLine, lines);
            }

            var arguments = _turnParametersToArgumentWithOptions
                .GetRecognizers(method)
                .ToArray();
            if (arguments.Any())
            {
                var lines = new List<string>
                {
                    string.Format("{0} {1}", method.Name, Description(type, method)),
                    string.Format("{0}:", AndAcceptTheFollowingParameters),
                    String.Join(", ", arguments.Select(DescriptionAndHelp)),
                    string.Format("{0}:", AndTheShortFormIs),
                    type.ControllerName() + " " + method.Name + " " +
                    String.Join(", ", arguments.Select(arg => arg.Argument.LongAlias().ToUpper()))
                };
                return string.Join(Environment.NewLine, lines);
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

        public string Help(string val = null, string action = null)
        {
            if (string.IsNullOrEmpty(val))
            {
                var lines = new List<string>();
                lines.Add(TheCommandsAre);
                lines.Add(String.Join(Environment.NewLine,
                           _classAndMethodRecognizers
                               .Where(cmr => cmr != typeof(HelpController))
                               .Select(cmr => "  " + HelpFor(cmr, true)).ToArray()));
                lines.Add(string.Empty);
                lines.Add(HelpCommandForMoreInformation);
                return string.Join(Environment.NewLine, lines);
            }
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
