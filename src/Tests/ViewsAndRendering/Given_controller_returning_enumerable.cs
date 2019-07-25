using System;
using System.IO;
using System.Linq;
using Isop;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests.ViewsAndRendering
{
    [TestFixture]
    public class Given_controller_returning_enumerable
    {
        class EnumerableController
        {
            public Func<object> OnEnumerate;
            public int Length;
            public System.Collections.IEnumerable Return()
            {
                for (int i = 0; i < Length; i++)
                {
                    yield return OnEnumerate();
                }
            }
        }
        
        
        [Test]
        public void It_understands_method_returning_enumerable()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new EnumerableController() { Length = 2, OnEnumerate = () => (count++) });

            var arguments = AppHostBuilder.Create(sc)
                .Recognize(typeof(EnumerableController))
                .BuildAppHost()
                .Parse(new[] { "Enumerable", "Return" });

            Assert.That(arguments.Unrecognized.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(2));
        }
    }
}