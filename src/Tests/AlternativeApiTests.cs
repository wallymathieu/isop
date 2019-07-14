using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Isop;
using Isop.Api;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tests.FakeControllers;

namespace Tests
{
    [TestFixture]
    public class AlternativeApiTests
    {
        [Test]
        public void It_can_parse_and_invoke()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci=>new MyController() { OnAction = (p1, p2, p3, p4) => (count++).ToString() });
            var arguments = Builder.Create(sc,new Configuration {
                CultureInfo= CultureInfo.InvariantCulture
            })
            .Recognize<MyController>()
            .BuildAppHost()
            .Controller("My")
            .Action("Action")
            .Parameters(new Dictionary<string, string> { { "param1", "value1" }, { "param2", "value2" }, { "param3", "3" }, { "param4", "3.4" } });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }
        [Test]
        public void It_can_get_help()
        {
            var help = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize<MyController>()
            .BuildAppHost()
            .Controller("My")
            .Action("Action")
            .Help();

            Assert.IsNotEmpty(help);
        }
    }
}
