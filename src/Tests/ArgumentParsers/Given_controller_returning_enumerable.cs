using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isop;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tests.FakeControllers;
namespace Tests.ArgumentParsers
{
    [TestFixture]
    public class Given_controller_returning_enumerable
    {
        [Test]
        public void It_understands_method_returning_enumerable()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new EnumerableController() { Length = 2, OnEnumerate = () => (count++) });

            var arguments = Builder.Create(sc)
                .Recognize(typeof(EnumerableController))
                .BuildAppHost()
                .Parse(new[] { "Enumerable", "Return" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(2));
        }
    }
}