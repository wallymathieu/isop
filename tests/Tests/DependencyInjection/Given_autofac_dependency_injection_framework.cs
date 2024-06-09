using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using With;

namespace Tests.DependencyInjection;
[TestFixture]
public class Given_autofac_dependency_injection_framework : Given_dependency_injection_framework
{
    class AutoFacRegistrationBuilder : BaseRegistrationBuilder
    {
        private readonly ContainerBuilder _containerBuilder = new ContainerBuilder().Tap(c =>
            c.Populate(new ServiceCollection().AddLogging()));
        public override void RegisterSingleton<T>(Func<T> factory) => _containerBuilder.Register(di => factory()).As<T>().SingleInstance();
        public override IServiceProvider Build() =>
            new AutofacServiceProvider(_containerBuilder.Build());
    }
    protected override BaseRegistrationBuilder RegistrationBuilder => new AutoFacRegistrationBuilder();
}
