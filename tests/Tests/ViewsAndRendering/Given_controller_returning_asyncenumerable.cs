#if NET8_0_OR_GREATER
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.ViewsAndRendering;

[TestFixture]
public class Given_controller_returning_asyncenumerable
{
    class EnumerableController<T>(Func<T> OnEnumerate, int Length)
    {
        public async IAsyncEnumerable<T> Return()
        {
            for (int i = 0; i < Length; i++)
            {
                await Task.CompletedTask;
                yield return OnEnumerate();
            }
        }
    }
    class EnumerableIntController(Func<int> OnEnumerate, int Length) :
        EnumerableController<int>(OnEnumerate, Length)
    { }
    class EnumerableObjectController(Func<object> OnEnumerate, int Length) :
        EnumerableController<object>(OnEnumerate, Length)
    { }

    [Test]
    public async Task It_understands_method_returning_enumerable_object()
    {
        var count = 0;
        var sc = new ServiceCollection();
        sc.AddSingleton(ci => new EnumerableObjectController(Length: 2, OnEnumerate: () => (count++)));

        var arguments = AppHostBuilder.Create(sc)
            .Recognize(typeof(EnumerableObjectController))
            .BuildAppHost()
            .Parse(new[] { "EnumerableObject", "Return" });

        Assert.That(arguments.Unrecognized.Count(), Is.EqualTo(0));
        var sw = new StringWriter();
        await arguments.InvokeAsync(sw);
        Assert.That(count, Is.EqualTo(2));
        Assert.That(sw.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries), Is.EqualTo(new[] { "0", "1" }));
    }

    [Test]
    public async Task It_understands_method_returning_enumerable_int()
    {
        var count = 0;
        var sc = new ServiceCollection();
        sc.AddSingleton(ci => new EnumerableIntController(Length: 2, OnEnumerate: () => (count++)));

        var arguments = AppHostBuilder.Create(sc)
            .Recognize(typeof(EnumerableIntController))
            .BuildAppHost()
            .Parse(new[] { "EnumerableInt", "Return" });

        Assert.That(arguments.Unrecognized.Count(), Is.EqualTo(0));
        var sw = new StringWriter();
        await arguments.InvokeAsync(sw);
        Assert.That(count, Is.EqualTo(2));
        Assert.That(sw.ToString().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries), Is.EqualTo(new[] { "0", "1" }));
    }
}

#endif