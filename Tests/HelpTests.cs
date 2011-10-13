using System;
using NUnit.Framework;

namespace Isop.Tests
{
    [TestFixture()]
    public class HelpTests
    {
        [Test]
        public void It_can_report_usage_for_simple_parameters ()
        {
            var usage = ArgumentParser.Build ()
                                    .Parameter ("beta", arg => { }, description:"Some description about beta")
                                    .Parameter ("alpha", arg => { })
                                    .RecognizeHelp ()
                                    .Help ();
            var tab = '\t';
            Assert.That (LineSplit (usage), Is.EquivalentTo (LineSplit (@"The arguments are:
  --beta" + tab + @"Some description about beta
  --alpha")));
        }

        [Test]
        public void It_can_report_usage_for_simple_parameters_with_different_texts ()
        {
            var usage = ArgumentParser.Build ()
                                    .Parameter ("beta", arg => { }, description:"Beskrivning av beta")
                                    .Parameter ("alpha", arg => { })
                                    .RecognizeHelp ()
                                    .HelpTextArgumentsAre ("Det finns följande argument:")
                                    .Help ();
            var tab = '\t';
            Assert.That (LineSplit (usage), Is.EquivalentTo (LineSplit (@"Det finns följande argument:
  --beta" + tab + @"Beskrivning av beta
  --alpha")));
        }

        internal class AnotherController
        {
            public void Action1 ()
            {
            }

            public void Action2 ()
            {
            }
        }

        [Test]
        public void It_can_report_usage_for_controllers ()
        {
            var usage = ArgumentParser.Build ()
                                    .Recognize (typeof(MyController))
                                    .Recognize (typeof(AnotherController))
                                    .RecognizeHelp ()
                                    .Help ();
            Assert.That (LineSplit (usage), Is.EquivalentTo (LineSplit (@"The commands are:
  My
  Another

Se 'COMMANDNAME' help <command> for more information")));
        }
  
        [Test]
        public void It_can_report_usage_for_controllers_and_have_a_different_help_text ()
        {
            var usage = ArgumentParser.Build ()
                .Recognize (typeof(MyController))
                .Recognize (typeof(AnotherController))
                .RecognizeHelp ()
                .HelpTextCommandsAre (
                    theCommandsAre:"Det finns följande kommandon:", 
                    helpCommandForMoreInformation:"Se 'Kommandonamn' help <kommando> för ytterligare information",
                    theSubCommandsFor:"Det finns föjande sub kommandon:",
                    helpSubCommandForMoreInformation:"Se 'Kommandonamn' help <kommando> <subkommando> för mer information")
                .Help ();
            Assert.That (LineSplit (usage), Is.EquivalentTo (LineSplit (@"Det finns följande kommandon:
  My
  Another

Se 'Kommandonamn' help <kommando> för ytterligare information")));
        }
        
        [Test]
        public void It_can_report_usage_for_a_specific_controller_and_have_a_different_help_text ()
        {
            var usage = ArgumentParser.Build ()
                .Recognize (typeof(MyController))
                .Recognize (typeof(AnotherController))
                .RecognizeHelp ()
                .HelpTextCommandsAre (
                    theCommandsAre:"Det finns följande kommandon:", 
                    helpCommandForMoreInformation:"Se 'Kommandonamn' help <kommando> för ytterligare information",
                    theSubCommandsFor:"Det finns föjande sub kommandon:",
                    helpSubCommandForMoreInformation:"Se 'Kommandonamn' help <kommando> <subkommando> för mer information")
                .HelpFor ("my");
            Assert.That (LineSplit (usage), Is.EquivalentTo (LineSplit (@"Det finns föjande sub kommandon:My
  Action

Se 'Kommandonamn' help <kommando> <subkommando> för mer information")));
        }
        
        private static string[] LineSplit (string usage)
        {
            return usage.Split (new []{"\r","\n"}, StringSplitOptions.RemoveEmptyEntries);
        }

        [Test]
        public void It_can_report_usage_when_no_parameters_given ()
        {
            var usage = ArgumentParser.Build ()
                                    .RecognizeHelp ()
                                    .Recognize (typeof(MyController))
                                    .Parse (new string[]{}).Invoke ();
            Assert.That (LineSplit (usage), Is.EquivalentTo (LineSplit (@"The commands are:
  My

Se 'COMMANDNAME' help <command> for more information")));
        }

        [Test]
        public void It_can_report_usage_for_controllers_and_actions ()
        {
            var usage = ArgumentParser.Build ()
                                    .Recognize (typeof(MyController))
                                    .Recognize (typeof(AnotherController))
                                    .RecognizeHelp ()
                                    .HelpFor ("Another");
            Assert.That (LineSplit (usage), Is.EquivalentTo (LineSplit (@"The sub commands for Another

  Action1
  Action2

Se 'COMMANDNAME' help <command> <subcommand> for more information")));
        }
     
        [Test]
        public void It_can_report_usage_for_controllers_with_description ()
        {
            var usage = ArgumentParser.Build ()
                                    .Recognize (typeof(DescriptionController))
                                    .RecognizeHelp ()
                                    .Help ();
            Assert.That (LineSplit (usage), Is.EquivalentTo (LineSplit (@"The commands are:
  Description  Some description

Se 'COMMANDNAME' help <command> for more information")));
        }
     
        [Test]
        public void It_can_report_usage_for_controllers_and_actions_with_description ()
        {
            var usage = ArgumentParser.Build ()
                                    .Recognize (typeof(DescriptionController))
                                    .RecognizeHelp ()
                                    .HelpFor ("Description");
            Assert.That (LineSplit (usage), Is.EquivalentTo (LineSplit (@"The sub commands for Description

  Action1  Some description 1
  Action2  Some description 2

Se 'COMMANDNAME' help <command> <subcommand> for more information")));
        }
     
     
        internal class DescriptionController
        {
            public void Action1 ()
            {
            }

            public void Action2 ()
            {
            }

            public string Help (string command)
            {
                switch (command) {
                case "Action1":
                    return "Some description 1";
                case "Action2":
                    return "Some description 2";
                default:
                    return "Some description";
                }
            }
        }
    }
}

