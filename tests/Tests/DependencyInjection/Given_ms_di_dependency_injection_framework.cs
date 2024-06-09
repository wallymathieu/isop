using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests.DependencyInjection;
[TestFixture]
public class Given_ms_di_dependency_injection_framework : Given_dependency_injection_framework
{
    protected override BaseRegistrationBuilder RegistrationBuilder => new MsRegistrationBuilder();

    class MsRegistrationBuilder : BaseRegistrationBuilder
    {
        IServiceCollection serviceCollection = new ServiceCollection().AddLogging();
        public override void RegisterSingleton<T>(Func<T> factory) =>
            serviceCollection.AddSingleton<T>(_ => factory());
        public override IServiceProvider Build() => serviceCollection.BuildServiceProvider();
    }
}
