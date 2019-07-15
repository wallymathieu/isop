using System;
using Lamar;
using NUnit.Framework;

namespace Tests.DependencyInjection
{
    [TestFixture]
    public class Given_lamar_dependency_injection_framework:Given_dependency_injection_framework
    {
        private Container _container ;

        protected override IServiceProvider ServiceProvider => _container.ServiceProvider;

        protected override void RegisterSingleton<T>(Func<T> factory)
        {
            // different style
            _container=new Container(x => { x.For<T>().Use(di=>factory()); });
        }
    }
}