using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests.DependencyInjection
{
    [TestFixture]
    public class Given_ms_di_dependency_injection_framework:Given_dependency_injection_framework
    {
        private ServiceCollection _serviceCollection = new ServiceCollection();
        protected override IServiceProvider ServiceProvider => _serviceCollection.BuildServiceProvider();
        protected override void RegisterSingleton<T>(Func<T> factory) => 
            _serviceCollection.AddSingleton(_ => factory());
    }
}