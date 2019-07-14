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
    public class ExceptionsTests
    {
        public class SpecificException : System.Exception
        {
        }

        [Test]
        public void It_can_parse_class_and_method_and_execute()
        {
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new ObjectController() { OnAction = () => throw new SpecificException() });

            var arguments = Builder.Create(sc, new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            }).Recognize<ObjectController>()
            .BuildAppHost()
            .Parse(new[] { "Object", "Action" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            Assert.Throws<SpecificException>(() =>arguments.Invoke(new StringWriter()));
        }
    }
}
