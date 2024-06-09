using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Isop;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tests.FakeControllers;

namespace Tests;

[TestFixture]
public class DisposeTests
{
    public class DisposeController : IDisposable
    {
        public DisposeController()
        {
        }

        public event EventHandler? OnDispose;
#pragma warning disable MA0069 // Non-constant static fields should not be visible
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static EventHandler? StaticOnDispose;
#pragma warning restore CA2211 // Non-constant fields should not be visible
#pragma warning restore MA0069 // Non-constant static fields should not be visible

        public string Method()
        {
            return "test";
        }

        public void Dispose()
        {
            OnDispose?.Invoke(this, EventArgs.Empty);
            StaticOnDispose?.Invoke(this, EventArgs.Empty);
        }
    }

    [Test]
    public async Task It_will_not_dispose_instances_set_to_be_recognized()
    {
        var count = 0;
        var c = new DisposeController();
        c.OnDispose += (_,_) => count++;
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(c);
        var build = AppHostBuilder.Create(serviceCollection, new AppHostConfiguration
        {
            CultureInfo = CultureInfo.InvariantCulture
        }).Recognize(typeof(DisposeController)).BuildAppHost();
        await build
            .Parse(["Dispose", "method"])
            .InvokeAsync(new StringWriter());
        Assert.That(count, Is.EqualTo(0));
    }
}
