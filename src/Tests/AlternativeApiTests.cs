using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Isop.Tests.FakeControllers;
using System.Globalization;
using System.IO;
namespace Isop.Tests
{
    [TestFixture]
    public class AlternativeApiTests
    {
        [Test]
        public void It_can_parse_and_invoke()
        {
            var count = 0;
            Func<Type, object> factory = (Type t) =>
            {
                Assert.That(t, Is.EqualTo(typeof(MyController)));
                return
                    (object)
                    new MyController() { OnAction = (p1, p2, p3, p4) => (count++).ToString() };
            };
            var arguments = new Build { typeof(MyController) }
                            .SetCulture(CultureInfo.InvariantCulture)
                            .SetFactory(factory)
                            .Controller("My")
                            .Action("Action")
                            .Parameters(new Dictionary<string, string> { { "param1", "value1" }, { "param2", "value2" }, { "param3", "3" }, { "param4", "3.4" } });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

    }
}
