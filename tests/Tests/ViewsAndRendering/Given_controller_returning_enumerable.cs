using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.ViewsAndRendering;

[TestFixture]
public class Given_controller_returning_enumerable
{
    class EnumerableController<T>(Func<T> OnEnumerate, int Length)
    {
        public System.Collections.IEnumerable Return()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return OnEnumerate();
            }
        }
    }
    class EnumerableController(Func<object> OnEnumerate, int Length) :
        EnumerableController<object>(OnEnumerate, Length)
    { }

    [Test]
    public async Task It_understands_method_returning_enumerable()
    {
        var count = 0;
        var sc = new ServiceCollection();
        sc.AddSingleton(ci => new EnumerableController(Length: 2, OnEnumerate: () => count++));

        var arguments = AppHostBuilder.Create(sc)
            .Recognize(typeof(EnumerableController))
            .BuildAppHost()
            .Parse(new[] { "Enumerable", "Return" });

        Assert.That(arguments.Unrecognized.Count(), Is.EqualTo(0));
        var sw = new StringWriter();
        await arguments.InvokeAsync(sw);
        Assert.That(count, Is.EqualTo(2));
        Assert.That(sw.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries), Is.EqualTo(new[] { "0", "1" }));
    }

    [Test]
    public async Task It_understands_method_returning_enumerable_of_string()
    {
        var count = 0;
        var sc = new ServiceCollection();
        sc.AddSingleton(ci => new EnumerableController(Length: 2, OnEnumerate: () => $"{count++}".ToString()));

        var arguments = AppHostBuilder.Create(sc)
            .Recognize(typeof(EnumerableController))
            .BuildAppHost()
            .Parse(new[] { "Enumerable", "Return" });

        Assert.That(arguments.Unrecognized.Count(), Is.EqualTo(0));
        var sw = new StringWriter();
        await arguments.InvokeAsync(sw);
        Assert.That(count, Is.EqualTo(2));
        Assert.That(sw.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries), Is.EqualTo(new[] { "0", "1" }));
    }
}
