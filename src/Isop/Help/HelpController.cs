using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Isop.Help
{
    using CommandLine.Help;
    using Domain;

    public class HelpController
    {
        private readonly HelpForArgumentWithOptions _helpForArgumentWithOptions;
        private readonly HelpForControllers _helpForClassAndMethod;

        public HelpController(
            HelpForArgumentWithOptions helpForArgumentWithOptions,
            HelpForControllers helpForClassAndMethod)
        {
            _helpForArgumentWithOptions = helpForArgumentWithOptions;
            _helpForClassAndMethod = helpForClassAndMethod;
        }

        public string Index()
        {
            var sb = new StringBuilder();
            if (_helpForArgumentWithOptions.CanHelp())
            {
                sb.AppendLine(_helpForArgumentWithOptions.Help());
            }
            if (_helpForClassAndMethod.CanHelp())
            {
                sb.AppendLine(_helpForClassAndMethod.Help());
            }
            return sb.ToString().Trim(' ', '\t', '\r', '\n') + Environment.NewLine;
        }

        public string Index(string command, string action)
        {
            if (String.IsNullOrEmpty(command))
                return Index();
            var sb = new StringBuilder();
            command = Regex.Replace(command, Conventions.ControllerName+"$", "", RegexOptions.IgnoreCase);
            if (_helpForArgumentWithOptions.CanHelp(command))
            {
                sb.AppendLine(_helpForArgumentWithOptions.Help(command));
            }
            if (_helpForClassAndMethod.CanHelp(command))
            {
                sb.AppendLine(_helpForClassAndMethod.Help(command, action));
            }
            return sb.ToString().Trim(' ', '\t', '\r', '\n') + Environment.NewLine;
        }
    }
}

