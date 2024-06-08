using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
namespace Isop.Tests
{
    //[TestFixture]
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

            var arguments = new Build(sc) { typeof(ObjectController) }
                                .SetCulture(CultureInfo.InvariantCulture)
                                .Parse(new[] { "Object", "Action" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            Assert.Throws<SpecificException>(() =>arguments.Invoke(new StringWriter()));
        }
    }
}
