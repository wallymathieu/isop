using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Tests.ArgumentParsers;
using Tests.FakeControllers;

namespace Tests;

[TestFixture]
public class Given_controller
{

    [Test]
    public async Task It_can_parse_class_and_method_and_execute()
    {
        var count = 0;
        var sc = new ServiceCollection();
        sc.AddSingleton(ci => new MyController { OnAction = (p1, p2, p3, p4) => (count++).ToString() });

        var arguments = AppHostBuilder.Create(sc, new AppHostConfiguration { CultureInfo = CultureInfo.InvariantCulture }).Recognize<MyController>()
                            .BuildAppHost()
                            .Parse(["My", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4"]);

        Assert.That(arguments.Unrecognized.Count, Is.EqualTo(0));
        await arguments.InvokeAsync(new StringWriter());
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task It_can_parse_class_and_method_and_execute_with_ordinal_syntax()
    {
        var count = 0;
        var sc = new ServiceCollection();
        sc.AddSingleton(ci => new MyController { OnAction = (p1, p2, p3, p4) => (count++).ToString() });
        var arguments = AppHostBuilder.Create(sc, new AppHostConfiguration { CultureInfo = CultureInfo.InvariantCulture }).Recognize<MyController>()
                        .BuildAppHost()
                        .Parse(new[] { "My", "Action", "value1", "value2", "3", "3.4" });

        Assert.That(arguments.Unrecognized.Count(), Is.EqualTo(0));
        await arguments.InvokeAsync(new StringWriter());
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void Ordinal_syntax_when_infer_is_turned_off()
    {
        var count = 0;
        var sc = new ServiceCollection();
        sc.AddSingleton(ci => new MyController { OnAction = (p1, p2, p3, p4) => (count++).ToString() });
        string[] args = ["My", "Action", "value1", "value2", "3", "3.4"];
        var parsed = AppHostBuilder.Create(sc, new AppHostConfiguration
        {
            DisableAllowInferParameter = true
        }).Recognize<MyController>()
            .BuildAppHost()
            .Parse(args);
        Assert.That(parsed.Recognized.Count, Is.EqualTo(0));
        CollectionAssert.AreEqual(
            args,
            parsed.Unrecognized.Select(u => u.Value));
    }

    [Test]
    public void It_can_parse_class_and_method_and_knows_whats_required()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton(ci => new MyController() { OnAction = (p1, p2, p3, p4) => "" });
        var build = AppHostBuilder.Create(sc).Recognize<MyController>()
                        .BuildAppHost();
        var expected = Helpers.DictionaryDescriptionToKv("[param1, True], [param2, True], [param3, True], [param4, True]", Boolean.Parse);

        var recognizers = build.Controller("My").Action("Action").Arguments;
        Assert.That(recognizers.Select(r => new KeyValuePair<string, bool>(r.Name, r.Required)).ToArray(),
            Is.EquivalentTo(expected.ToArray()));
    }

    [Test]
    public void It_can_parse_class_and_method_and_fail()
    {
        var builder = AppHostBuilder.Create(new AppHostConfiguration { CultureInfo = CultureInfo.InvariantCulture }).Recognize<MyController>()
            .BuildAppHost();

        Assert.ThrowsAsync<MissingArgumentException>(async () => await builder
            .Parse(["My", "Action", "--param2", "value2", "--paramX", "3", "--param1", "value1", "--param4", "3.4"])
            .InvokeAsync(Console.Out));
    }

    [Test]
    public void It_can_parse_class_and_method_and_fail_because_no_arguments_given()
    {
        var builder = AppHostBuilder.Create().Recognize<MyController>()
            .BuildAppHost();

        Assert.ThrowsAsync<MissingArgumentException>(async () => await builder
            .Parse(["My", "Action"])
            .InvokeAsync(Console.Out));
    }
}
