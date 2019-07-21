using System;
using System.Collections.Generic;
using System.IO;
using Isop;
using NUnit.Framework;
using Tests.FakeControllers;

namespace Tests.DependencyInjection
{
    public abstract class Given_dependency_injection_framework
    {
        protected abstract IServiceProvider ServiceProvider { get; }
        protected abstract void RegisterSingleton<T>(Func<T> factory) where T : class;
        private int count;
        [SetUp]
        public void SetUp()
        {
            count = 0;
            RegisterSingleton(()=>new MyController { OnAction = (p1, p2, p3, p4) => (count++).ToString() });
        }
        
        [Test]
        public void It_can_parse_and_invoke()
        {
            Builder.Create(ServiceProvider)
                .Recognize<MyController>()
                .BuildAppHost()
                .Controller("My")
                .Action("Action")
                .Parameters(new Dictionary<string, string> { { "param1", "value1" }, { "param2", "value2" }, { "param3", "3" }, { "param4", "3.4" } })
                .Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }
    }
}