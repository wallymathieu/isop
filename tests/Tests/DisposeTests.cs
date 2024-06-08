using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Isop;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tests.FakeControllers;

namespace Tests
{

    [TestFixture]
    public class DisposeTests
    {
        public class DisposeController:IDisposable
        {
            public DisposeController()
            {
            }

            public event Action? OnDispose;
            public static event Action<DisposeController>? StaticOnDispose;

            public string Method()
            {
                return "test";
            }

            public void Dispose()
            {
                OnDispose?.Invoke();
                StaticOnDispose?.Invoke(this);
            }
        }
        
        [Test]
        public async Task It_will_not_dispose_instances_set_to_be_recognized()
        {
            var count = 0;
            var c = new DisposeController();
            c.OnDispose += () => count++;
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(c);
            var build = AppHostBuilder.Create(serviceCollection,new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            }).Recognize(typeof(DisposeController)).BuildAppHost();
            await build
                .Parse(["Dispose", "method"])
                .InvokeAsync(new StringWriter());
            Assert.That(count, Is.EqualTo(0));
        }
    }
}
