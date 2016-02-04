namespace Isop.Help
{
    public class HelpTexts
    {
        public HelpTexts()
        {
            HelpSubCommandForMoreInformation = "See 'COMMANDNAME' help <command> <subcommand> for more information";

            HelpCommandForMoreInformation = "See 'COMMANDNAME' help <command> for more information";
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
    
}
