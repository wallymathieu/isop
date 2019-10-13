using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Lamar;
using NUnit.Framework;

namespace Tests.DependencyInjection
{
    [TestFixture]
    public class Given_lamar_dependency_injection_framework:Given_dependency_injection_framework
    {
        class LamarRegistrationBuilder : RegistrationBuilder
        {
            private List<Expression<Action<ServiceRegistry>>> expr = new List<Expression<Action<ServiceRegistry>>>();
            public override void RegisterSingleton<T>(Func<T> factory) => expr.Add(x=>x.For<T>().Use(di => factory()));
            public override IServiceProvider Build() =>
                new Container(x => { Apply(x); }).ServiceProvider;

            private void Apply(ServiceRegistry x)
            {
                foreach (var ex in expr)
                {
                    ex.Compile().Invoke(x);
                }
            }
        }

        protected override RegistrationBuilder RegistrationBuilder => new LamarRegistrationBuilder();
    }
}