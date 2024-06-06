using System;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace Isop.Help
{
    using Localization;
    using CommandLine.Help;
    using Domain;

    public class HelpController(IOptions<Texts> texts,
        Recognizes recognizes,
        IOptions<Configuration> config,
        IServiceProvider serviceProvider,
        IOptions<Conventions> conventions)
    {
        private readonly Conventions _conventions = conventions.Value ?? throw new ArgumentNullException(nameof(conventions));
        private readonly HelpForArgumentWithOptions _helpForArgumentWithOptions = new HelpForArgumentWithOptions(texts, recognizes, config);
        private readonly HelpForControllers _helpForClassAndMethod = new HelpForControllers(recognizes,
                new HelpXmlDocumentation(), texts, config, serviceProvider, conventions);

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

        public string Index(string controller, string? action)
        {
            if (string.IsNullOrEmpty(controller))
                return Index();
            var sb = new StringBuilder();
            controller = Regex.Replace(controller, _conventions.ControllerName+"$", "", RegexOptions.IgnoreCase);
            if (_helpForArgumentWithOptions.CanHelp(controller))
            {
                sb.AppendLine(_helpForArgumentWithOptions.Help(controller));
            }
            if (_helpForClassAndMethod.CanHelp(controller))
            {
                sb.AppendLine(_helpForClassAndMethod.Help(controller, action));
            }
            return sb.ToString().Trim(' ', '\t', '\r', '\n') + Environment.NewLine;
        }
    }
}

