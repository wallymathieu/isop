using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ConsoleHelpers
{
    [TestFixture]
    public class ArgumentParserTests
    {
        [SetUp]
        public void SetUp() { }
        [TearDown]
        public void TearDown() { }

        [Test]
        public void Recognizes_shortform()
        {
            var arg = new Argument("argument");

            var parser = new ArgumentParser(new[] { "-a" }, new[] { arg });
            var arguments = parser.GetInvokedArguments();
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument, Is.EqualTo(arg));
        }

        [Test]
        public void Given_several_arguments_Then_the_correct_one_is_recognized()
        {
            var arguments = GetParsedArgumentsForSingleArgumentRecognizer("beta", new[] { "-a", "-b" });

            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Parameter, Is.EqualTo("-b"));
        }

        [Test]
        public void Recognizes_longform()
        {
            var arguments = GetParsedArgumentsForSingleArgumentRecognizer("beta", new[] { "-a", "--beta" });
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Parameter, Is.EqualTo("--beta"));
        }

        private static IEnumerable<ArgumentWithParameters> GetParsedArgumentsForSingleArgumentRecognizer(string longname, IEnumerable<string> arg)
        {
            var parser = new ArgumentParser(arg, new[] { new Argument(longname) });
            return parser.GetInvokedArguments();
        }

        [Test]
        public void It_can_parse_parameter_value()
        {
            var arguments = GetParsedArgumentsForSingleArgumentRecognizer("beta", new []{"-a","--beta", "value"});
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Parameter, Is.EqualTo("--beta"));
            Assert.That(arg1.Value, Is.EqualTo("value"));
        }
    }
}