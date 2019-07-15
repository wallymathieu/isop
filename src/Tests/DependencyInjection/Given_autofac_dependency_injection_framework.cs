using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using With;

namespace Tests.DependencyInjection
{
    [TestFixture]
    public class Given_autofac_dependency_injection_framework:Given_dependency_injection_framework
    { 
        IServiceCollection serviceCollection = new ServiceCollection().AddLogging();
        private ContainerBuilder _containerBuilder = new ContainerBuilder();
        protected override IServiceProvider ServiceProvider => 
            new AutofacServiceProvider(_containerBuilder.Build().BeginLifetimeScope(b=>b.Populate(serviceCollection) ));
        protected override void RegisterSingleton<T>(Func<T> factory)
        {
            _containerBuilder.Register(di=>factory()).As<T>();
        }
    }
}