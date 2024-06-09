﻿using System.IO;
using Isop.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tests.FakeControllers;
using With;

namespace Tests.Docs;
[TestFixture]
public class HelpTests
{
    [Test]
    public async Task It_can_report_usage_for_simple_parameters()
    {
        var usage = await AppHostBuilder.Create(new AppHostConfiguration
        {
            CultureInfo = CultureInfo.InvariantCulture
        })
        .Parameter("beta", arg => { }, description: "Some description about beta")
        .Parameter("alpha", arg => { })
        .BuildAppHost()
        .HelpAsync();
        var tab = '\t';
        Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The arguments are:
  --beta" + tab + @"Some description about beta
  --alpha")));
    }

    [Test]
    public async Task It_can_report_usage_for_simple_parameters_with_different_texts()
    {
        var usage = await AppHostBuilder.Create(
                new ServiceCollection()
                    .Tap(s => s.AddSingleton(Options.Create(
                        new Texts { TheArgumentsAre = "Det finns följande argument:" })))
                , new AppHostConfiguration
                {
                    CultureInfo = CultureInfo.InvariantCulture
                })
        .Parameter("beta", arg => { }, description: "Beskrivning av beta")
        .Parameter("alpha", arg => { })
        .BuildAppHost()
        .HelpAsync();
        var tab = '\t';
        Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"Det finns följande argument:
  --beta" + tab + @"Beskrivning av beta
  --alpha")));
    }

    [Test]
    public async Task It_can_report_usage_for_controllers()
    {
        var usage = await AppHostBuilder.Create(new AppHostConfiguration
        {
            CultureInfo = CultureInfo.InvariantCulture
        })
        .Recognize(typeof(MyController))
        .Recognize(typeof(AnotherController))
        .BuildAppHost().HelpAsync();
        Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The commands are:
  My
  Another

See 'COMMANDNAME' help <command> for more information")));
    }

    [Test]
    public async Task It_can_report_usage_for_controllers_when_having_required_parameters()
    {
        var usage = await AppHostBuilder.Create(new AppHostConfiguration
        {
            CultureInfo = CultureInfo.InvariantCulture
        })
        .Parameter("required", required: true)
        .Recognize(typeof(MyController))
        .Recognize(typeof(AnotherController))
        .BuildAppHost()
        .HelpAsync();
        Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The arguments are:
  --required
The commands are:
  My
  Another

See 'COMMANDNAME' help <command> for more information")));
    }

    [Test]
    public async Task It_can_report_usage_for_controllers_and_have_a_different_help_text()
    {
        var usage = await AppHostBuilder.Create(
                new ServiceCollection().Tap(s => s.AddSingleton(
                    Options.Create(new Texts()
                    {
                        TheCommandsAre = "Det finns följande kommandon:",
                        HelpCommandForMoreInformation = "Se 'Kommandonamn' help <kommando> för ytterligare information",
                        TheSubCommandsFor = "Det finns föjande sub kommandon:",
                        HelpSubCommandForMoreInformation =
                        "Se 'Kommandonamn' help <kommando> <subkommando> för mer information"
                    }))),
                new AppHostConfiguration
                {
                    CultureInfo = CultureInfo.InvariantCulture
                })
        .Recognize(typeof(MyController))
        .Recognize(typeof(AnotherController))
        .BuildAppHost()
        .HelpAsync();
        Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"Det finns följande kommandon:
  My
  Another

Se 'Kommandonamn' help <kommando> för ytterligare information")));
    }

    [Test]
    public void It_can_report_usage_for_a_specific_controller_and_have_a_different_help_text()
    {
        var usage = AppHostBuilder.Create(
                new ServiceCollection().Tap(s => s.AddSingleton(Options.Create(new Texts()
                {
                    TheCommandsAre = "Det finns följande kommandon:",
                    HelpCommandForMoreInformation = "Se 'Kommandonamn' help <kommando> för ytterligare information",
                    TheSubCommandsFor = "Det finns föjande sub kommandon:",
                    HelpSubCommandForMoreInformation =
                    "Se 'Kommandonamn' help <kommando> <subkommando> för mer information"
                }))),
                new AppHostConfiguration
                {
                    CultureInfo = CultureInfo.InvariantCulture
                })
        .Recognize(typeof(MyController))
        .Recognize(typeof(AnotherController))

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
        var usage = AppHostBuilder.Create(
                new ServiceCollection().Tap(s => s.AddSingleton(Options.Create(new Texts
                {
                    TheCommandsAre = "Det finns följande kommandon:",
                    HelpCommandForMoreInformation = "Se 'Kommandonamn' help <kommando> för ytterligare information",
                    TheSubCommandsFor = "Det finns föjande sub kommandon:",
                    HelpSubCommandForMoreInformation =
                    "Se 'Kommandonamn' help <kommando> <subkommando> för mer information",
                    AndAcceptTheFollowingParameters = "Och accepterar följande parametrar",
                    AndTheShortFormIs = "Och kortformen är"
                }))),
                new AppHostConfiguration
                {
                    CultureInfo = CultureInfo.InvariantCulture
                })
        .Recognize(typeof(MyController))
        .Recognize(typeof(AnotherController))
        .BuildAppHost()
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
    public async Task It_can_report_usage_when_no_parameters_given()
    {
        var cout = new StringWriter();
        await AppHostBuilder.Create(new AppHostConfiguration
        {
            CultureInfo = CultureInfo.InvariantCulture
        })
        .Recognize(typeof(MyController))
        .BuildAppHost()
        .Parse([]).InvokeAsync(cout);
        Assert.That(LineSplit(cout.ToString()), Is.EquivalentTo(LineSplit(@"The commands are:
  My

See 'COMMANDNAME' help <command> for more information")));
    }

    [Test]
    public void It_can_report_usage_for_controllers_and_actions()
    {
        var usage = AppHostBuilder.Create(new AppHostConfiguration
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
        var usage = AppHostBuilder.Create(new AppHostConfiguration
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
    public async Task It_can_report_usage_for_controllers_with_description()
    {
        var usage = await AppHostBuilder.Create(new AppHostConfiguration
        {
            CultureInfo = CultureInfo.InvariantCulture
        })
        .Recognize(typeof(DescriptionController))
        .BuildAppHost()
        .HelpAsync();
        Assert.That(LineSplit(usage), Is.EquivalentTo(LineSplit(@"The commands are:
  Description  Some description

See 'COMMANDNAME' help <command> for more information")));
    }

    [Test]
    public void It_can_report_usage_for_controllers_and_actions_with_description()
    {
        var usage = AppHostBuilder.Create(new AppHostConfiguration
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
        var usage = AppHostBuilder.Create(new AppHostConfiguration
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
        var usage = AppHostBuilder.Create(new AppHostConfiguration
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
        var usage = AppHostBuilder.Create(new AppHostConfiguration
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
    public void It_can_report_usage_for_controllers_and_actions_with_description_in_comments()
    {
        var usage = AppHostBuilder.Create(new AppHostConfiguration
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


