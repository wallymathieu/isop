using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Isop;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Tests.FakeControllers;

namespace Tests.DependencyInjection;

public abstract class Given_dependency_injection_framework
{
    public abstract class BaseRegistrationBuilder
    {
        public abstract void RegisterSingleton<T>(Func<T> factory) where T : class;
        public abstract IServiceProvider Build();
    }
    protected IServiceProvider? ServiceProvider;
    private int count;
    [SetUp]
    public void SetUp()
    {
        count = 0;
        var registrationBuilder = RegistrationBuilder;
        registrationBuilder.RegisterSingleton(() => new MyController { OnAction = (p1, p2, p3, p4) => count++.ToString() });
        registrationBuilder.RegisterSingleton(() => Options.Create(new AppHostConfiguration { CultureInfo = CultureInfo.InvariantCulture }));
        ServiceProvider = registrationBuilder.Build();
    }

    protected abstract BaseRegistrationBuilder RegistrationBuilder { get; }

    [Test]
    public async Task It_can_parse_and_invoke()
    {
        await AppHostBuilder.Create(ServiceProvider!)
            .Recognize<MyController>()
            .BuildAppHost()
            .Controller("My")
            .Action("Action")
            .Parameters(new Dictionary<string, string?> { { "param1", "value1" }, { "param2", "value2" }, { "param3", "3" }, { "param4", "3.4" } })
            .InvokeAsync(new StringWriter());
        Assert.That(count, Is.EqualTo(1));
    }
}
