using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Isop.Tests.FakeControllers;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Isop.Tests
{
    //[TestFixture]
    public class AlternativeApiTests
    {
        [Test]
        public void It_can_parse_and_invoke()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci=>new MyController() { OnAction = (p1, p2, p3, p4) => (count++).ToString() });
            var arguments = new Build(sc) { typeof(MyController) }
                            .SetCulture(CultureInfo.InvariantCulture)
                            //.SetFactory(factory)
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
            var help = new Build() { typeof(MyController) }
                            .SetCulture(CultureInfo.InvariantCulture)
                            .ShouldRecognizeHelp()
                            .Controller("My")
                            .Action("Action")
                            .Help();

            Assert.That(help, Is.EqualTo("ActionHelp"));
        }
    }
}
