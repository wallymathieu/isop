using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Isop.Tests.FakeControllers;
using NUnit.Framework;

namespace Isop.Tests
{
    [TestFixture()]
    public class HelpTests
    {
        [Test]
        public void It_can_report_usage_for_simple_parameters()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Parameter("beta", arg => { }, description: "Some description about beta")
            .Parameter("alpha", arg => { })
            .BuildAppHost()
            .Help();
            var tab = '\t';
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The arguments are:
  --beta" + tab + @"Some description about beta
  --alpha")));
        }

        [Test]
        public void It_can_report_usage_for_simple_parameters_with_different_texts()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Parameter("beta", arg => { }, description: "Beskrivning av beta")
            .Parameter("alpha", arg => { })
            .WithHelpTexts(h =>
            {
                h.TheArgumentsAre = "Det finns följande argument:";
            })
            .BuildAppHost()
            .Help();
            var tab = '\t';
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"Det finns följande argument:
  --beta" + tab + @"Beskrivning av beta
  --alpha")));
        }

        [Test]
        public void It_can_report_usage_for_controllers()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(MyController))
            .Recognize(typeof(AnotherController))
            .BuildAppHost().Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The commands are:
  My
  Another

See 'COMMANDNAME' help <command> for more information")));
        }

        [Test]
        public void It_can_report_usage_for_controllers_when_having_required_parameters()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Parameter("required", required: true)
            .Recognize(typeof(MyController))
            .Recognize(typeof(AnotherController))
            .BuildAppHost()
            .Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The arguments are:
  --required
The commands are:
  My
  Another

See 'COMMANDNAME' help <command> for more information")));
        }

        [Test]
        public void It_can_report_usage_for_controllers_and_have_a_different_help_text()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            }).WithHelpTexts(h =>
            {
                h.TheCommandsAre = "Det finns följande kommandon:";
                h.HelpCommandForMoreInformation = "Se 'Kommandonamn' help <kommando> för ytterligare information";
                h.TheSubCommandsFor = "Det finns föjande sub kommandon:";
                h.HelpSubCommandForMoreInformation =
                    "Se 'Kommandonamn' help <kommando> <subkommando> för mer information";
            })
            .Recognize(typeof(MyController))
            .Recognize(typeof(AnotherController))
            .BuildAppHost()
            .Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"Det finns följande kommandon:
  My
  Another

Se 'Kommandonamn' help <kommando> för ytterligare information")));
        }

        [Test]
        public void It_can_report_usage_for_a_specific_controller_and_have_a_different_help_text()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(MyController))
            .Recognize(typeof(AnotherController))
            .WithHelpTexts(h =>
            {
                h.TheCommandsAre = "Det finns följande kommandon:";
                h.HelpCommandForMoreInformation = "Se 'Kommandonamn' help <kommando> för ytterligare information";
                h.TheSubCommandsFor = "Det finns föjande sub kommandon:";
                h.HelpSubCommandForMoreInformation =
                    "Se 'Kommandonamn' help <kommando> <subkommando> för mer information";
            })
            .BuildAppHost()
            .Controller("my")
            .Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"Det finns föjande sub kommandon:My
  Action  ActionHelp --param1 --param2 --param3 --param4

Se 'Kommandonamn' help <kommando> <subkommando> för mer information")));
        }

        [Test]
        public void It_can_report_usage_for_a_specific_controller_and_action_and_have_a_different_help_text()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(MyController))
            .Recognize(typeof(AnotherController))
                .WithHelpTexts(h =>
                {
                    h.TheCommandsAre = "Det finns följande kommandon:";
                    h.HelpCommandForMoreInformation = "Se 'Kommandonamn' help <kommando> för ytterligare information";
                    h.TheSubCommandsFor = "Det finns föjande sub kommandon:";
                    h.HelpSubCommandForMoreInformation =
                        "Se 'Kommandonamn' help <kommando> <subkommando> för mer information";
                    h.AndAcceptTheFollowingParameters = "Och accepterar följande parametrar";
                    h.AndTheShortFormIs = "Och kortformen är";
                }).BuildAppHost()
            .Controller("my")
            .Action("Action")
            .Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"Action   ActionHelp
Och accepterar följande parametrar:
--param1, --param2, --param3, --param4
Och kortformen är:
My Action PARAM1, PARAM2, PARAM3, PARAM4")));
        }

        private static string[] LineSplit(string usage)
        {
            return usage.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToArray();
        }

        [Test]
        public void It_can_report_usage_when_no_parameters_given()
        {
            var cout = new StringWriter();
            Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(MyController))
            .BuildAppHost()
            .Parse(new string[] { }).Invoke(cout);
            Assert.That(LineSplit(cout.ToString()), Is.EquivalentTo(LineSplit(@"The commands are:
  My

See 'COMMANDNAME' help <command> for more information")));
        }

        [Test]
        public void It_can_report_usage_for_controllers_and_actions()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(MyController))
            .Recognize(typeof(AnotherController))
            .BuildAppHost()
            .Controller("Another")
            .Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The sub commands for Another

  Action1  --param1
  Action2

See 'COMMANDNAME' help <command> <subcommand> for more information")));
        }

        [Test]
        public void It_can_report_usage_for_controller_and_action()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(MyController))
            .Recognize(typeof(AnotherController))
            .BuildAppHost()
            .Controller("Another")
            .Action("Action1")
            .Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"Action1
And accept the following parameters:
--param1
And the short form is:
Another Action1 PARAM1")));
        }

        [Test]
        public void It_can_report_usage_for_controllers_with_description()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(DescriptionController))
            .BuildAppHost()
            .Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The commands are:
  Description  Some description

See 'COMMANDNAME' help <command> for more information")));
        }

        [Test]
        public void It_can_report_usage_for_controllers_and_actions_with_description()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(DescriptionController))
            .BuildAppHost()
            .Controller("Description")
            .Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The sub commands for Description

  Action1  Some description 1
  Action2  Some description 2

See 'COMMANDNAME' help <command> <subcommand> for more information")));
        }

        [Test]
        public void It_can_report_usage_for_actions_with_description()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(DescriptionController))
            .BuildAppHost()
            .Controller("Description")
            .Action("action1")
            .Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"Action1   Some description 1")));
        }

        [Test]
        public void It_can_report_usage_for_missing_action()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(DescriptionController))
            .BuildAppHost()
            .Controller("Description")
            .Action("actionX")
            .Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"Unknown action
actionX")));
        }

        [Test]
        public void It_can_report_usage_for_controllers_and_actions_with_fullname()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(DescriptionController))
            .BuildAppHost()
            .Controller("DescriptionController")
            .Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The sub commands for Description

  Action1  Some description 1
  Action2  Some description 2

See 'COMMANDNAME' help <command> <subcommand> for more information")));
        }

        [Test]
        public void It_can_report_usage_for_controllers_and_actions_with_description_in_comments()
        {
            var usage = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize(typeof(DescriptionWithCommentsController))
            .BuildAppHost()
            .Controller("DescriptionWithComments")
            .Help();
            Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The sub commands for DescriptionWithComments

  Action1  Some description 1
  Action2  Some description 2
  Action3  Some description 3 --param1 --param2
See 'COMMANDNAME' help <command> <subcommand> for more information")));
        }
    }
}

