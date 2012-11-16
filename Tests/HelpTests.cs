using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Isop.Tests
{
    [TestFixture()]
    public class HelpTests
    {
        [Test]
        public void It_can_report_usage_for_simple_parameters ()
        {
            var usage = new Build()
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
            var usage = new Build()
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
            public void Action1 (string param1)
            {
            }

            public void Action2 ()
            {
            }
        }

        [Test]
        public void It_can_report_usage_for_controllers ()
        {
            var usage = new Build()
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
        public void It_can_report_usage_for_controllers_when_having_required_parameters ()
        {
            var usage = new Build()
                                    .Parameter("required",required:true)
                                    .Recognize (typeof(MyController))
                                    .Recognize (typeof(AnotherController))
                                    .RecognizeHelp ()
                                    .Help ();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The arguments are:
  --required
The commands are:
  My
  Another

Se 'COMMANDNAME' help <command> for more information")));
        }
  
        [Test]
        public void It_can_report_usage_for_controllers_and_have_a_different_help_text ()
        {
            var usage = new Build()
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
            var usage = new Build()
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
            return usage.Split (new []{"\r","\n"}, StringSplitOptions.RemoveEmptyEntries).Select(l=>l.Trim()).ToArray();
        }

        [Test]
        public void It_can_report_usage_when_no_parameters_given ()
        {
            var cout = new StringWriter();
            new Build()
                                    .RecognizeHelp ()
                                    .Recognize (typeof(MyController))
                                    .Parse (new string[]{}).Invoke (cout);
            Assert.That (LineSplit (cout.ToString()), Is.EquivalentTo (LineSplit (@"The commands are:
  My

Se 'COMMANDNAME' help <command> for more information")));
        }

        [Test]
        public void It_can_report_usage_for_controllers_and_actions ()
        {
            var usage = new Build()
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
        public void It_can_report_usage_for_controller_and_action()
        {
            var usage = new Build()
                                    .Recognize(typeof(MyController))
                                    .Recognize(typeof(AnotherController))
                                    .RecognizeHelp()
                                    .HelpFor("Another", "Action1");
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"Action1
And accept the following parameters:
--param1")));
        }
     
        [Test]
        public void It_can_report_usage_for_controllers_with_description ()
        {
            var usage = new Build()
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
            var usage = new Build()
                                    .Recognize (typeof(DescriptionController))
                                    .RecognizeHelp ()
                                    .HelpFor ("Description");
            Assert.That (LineSplit (usage), Is.EquivalentTo (LineSplit (@"The sub commands for Description

  Action1  Some description 1
  Action2  Some description 2

Se 'COMMANDNAME' help <command> <subcommand> for more information")));
        }

        [Test]
        public void It_can_report_usage_for_actions_with_description()
        {
            var usage = new Build()
                                    .Recognize(typeof(DescriptionController))
                                    .RecognizeHelp()
                                    .HelpFor("Description","action1");
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"Action1   Some description 1")));
        }

        [Test]
        public void It_can_report_usage_for_controllers_and_actions_with_fullname()
        {
            var usage = new Build()
                                    .Recognize(typeof(DescriptionController))
                                    .RecognizeHelp()
                                    .HelpFor("DescriptionController");
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The sub commands for Description

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

        [Test
        //, Ignore("need some way of testing this with albacore, resharper ... shadow copy")
        ]
        public void It_can_report_usage_for_controllers_and_actions_with_description_in_comments ()
        {
            var usage = new Build()
                                    .Recognize (typeof(DescriptionWithCommentsController))
                                    .RecognizeHelp ()
                                    .HelpFor ("DescriptionWithComments");
            Assert.That (LineSplit (usage), Is.EquivalentTo (LineSplit (@"The sub commands for DescriptionWithComments

  Action1  Some description 1
  Action2  Some description 2

Se 'COMMANDNAME' help <command> <subcommand> for more information")));
        }
        
        /// <summary>
        /// Some description
        /// </summary>
        internal class DescriptionWithCommentsController
        {
            /// <summary>
            /// Some description 1
            /// </summary>
            public void Action1 ()
            {
            }
            /// <summary>
            /// Some description 2
            /// </summary>
            public void Action2 ()
            {
            }
        }
        
        [Test] public void Can_read_xml_doc()
        {
            var doc = new HelpXmlDocumentation().GetSummariesFromText (File.ReadAllText("Tests.xml"));
            Assert.That(doc["P:Isop.Tests.FullConfiguration.Global"],Is.EqualTo("GLOBAL!!"));
        }
        
        [Test] public void Can_get_same_key_as_in_xmldoc()
        {
            var helpXml = new HelpXmlDocumentation();
            var finder = new MethodInfoFinder();
            var _global = finder.MatchGet(typeof(FullConfiguration).GetMethods(),"Global");
            Assert.That(helpXml.GetKey(_global.MethodInfo),Is.EqualTo("P:Isop.Tests.FullConfiguration.Global"));
            var action1 = finder.Match(typeof(DescriptionWithCommentsController).GetMethods(),name:"Action1");
            Assert.That(helpXml.GetKey(action1),Is.EqualTo("M:Isop.Tests.HelpTests.DescriptionWithCommentsController.Action1"));
            
        }
    }
}

