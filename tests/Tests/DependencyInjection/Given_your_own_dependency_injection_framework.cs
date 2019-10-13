using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests.DependencyInjection
{
    [TestFixture]
    public class Given_your_own_dependency_injection_framework:Given_dependency_injection_framework
    {
        protected override RegistrationBuilder RegistrationBuilder => new MyOwnContainer();

        class MyOwnContainer: RegistrationBuilder, IServiceScopeFactory
        {
            private readonly IDictionary<Type, Func<object>> _registrations;
            private IServiceProvider _scp;

            public MyOwnContainer() =>
                _registrations = new Dictionary<Type,Func<object>>
                {
                    {typeof(IServiceScopeFactory), ()=>this}
                };

            
            public IServiceScope CreateScope() => new SimpleScope(new ServiceProviderImpl(_registrations));

            public override void RegisterSingleton<T>(Func<T> factory) => _registrations.Add(typeof(T), () => factory());

            public override IServiceProvider Build() => _scp ?? (_scp = new ServiceProviderImpl(_registrations));

            class SimpleScope : IServiceScope
            {
                public SimpleScope(IServiceProvider serviceProvider)
                {
                    ServiceProvider = serviceProvider;
                }

                public void Dispose()
                {
                }

                public IServiceProvider ServiceProvider { get; }
            }
            class ServiceProviderImpl:IServiceProvider
            {
                private readonly IDictionary<Type, Func<object>> _registrations;

                public ServiceProviderImpl(IDictionary<Type, Func<object>> registrations) => _registrations = registrations;

                public object GetService(Type serviceType) => _registrations.TryGetValue(serviceType, out var factory)
                    ?factory.Invoke()
                    :null;
            }
        }
    }
}