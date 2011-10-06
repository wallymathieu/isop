using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers.Console
{
    public class HelpController
    {
        HelpForArgumentWithOptions helpForArgumentWithOptions;
        HelpForClassAndMethod helpForClassAndMethod;
        public HelpController(HelpForArgumentWithOptions helpForArgumentWithOptions, HelpForClassAndMethod helpForClassAndMethod)
        {
            this.helpForArgumentWithOptions = helpForArgumentWithOptions;
            this.helpForClassAndMethod = helpForClassAndMethod;
        }

        public string Index()
        {
            var sb = new StringBuilder();
            if (helpForArgumentWithOptions.CanHelp())
            {
              sb.AppendLine(helpForArgumentWithOptions.Help());
            }
            if (helpForClassAndMethod.CanHelp())
            {
                sb.AppendLine(helpForClassAndMethod.Help());
            }
            return sb.ToString().Trim(' ','\t','\r','\n');
        }
    }

    public class HelpForArgumentWithOptions
    {
        readonly IEnumerable<ArgumentWithOptions> argumentWithOptionses;
        public string TheArgumentsAre { get; set; }

        public HelpForArgumentWithOptions(IEnumerable<ArgumentWithOptions> argumentWithOptionses)
        {
            this.argumentWithOptionses = argumentWithOptionses;
            TheArgumentsAre = "The arguments are:";
        }

        public string Help()
        {
            return TheArgumentsAre + Environment.NewLine +
                   String.Join(Environment.NewLine,
                               argumentWithOptionses.Select(ar => "  " + ar.Help()).ToArray());
        }

        public bool CanHelp()
        {
            return argumentWithOptionses.Any();
        }
    }
    public class HelpForClassAndMethod
    {
        readonly IEnumerable<ClassAndMethodRecognizer> classAndMethodRecognizers;
        public string TheCommandsAre { get; set; }
        public string HelpCommandForMoreInformation { get; set; }

        public string HelpSubCommandForMoreInformation { get; set; }

        public HelpForClassAndMethod(IEnumerable<ClassAndMethodRecognizer> classAndMethodRecognizers)
        {
            this.classAndMethodRecognizers = classAndMethodRecognizers;

            HelpSubCommandForMoreInformation = "Se 'COMMANDNAME' help <command> <subcommand> for more information";

            HelpCommandForMoreInformation = "Se 'COMMANDNAME' help <command> for more information";
            TheCommandsAre = "The commands are:";
        }

        public string Help()
        {
            return TheCommandsAre + Environment.NewLine +
                   String.Join(Environment.NewLine,
                               classAndMethodRecognizers.Select(cmr => "  " + cmr.Help(true)).ToArray())
                   + Environment.NewLine
                   + Environment.NewLine
                   + HelpCommandForMoreInformation;
        }

        public string Help(IEnumerable<ClassAndMethodRecognizer> classAndMethodRecognizers, string command)
        {
            throw new NotImplementedException();
        }

        public bool CanHelp()
        {
            return classAndMethodRecognizers.Any();
        }
    }
}
