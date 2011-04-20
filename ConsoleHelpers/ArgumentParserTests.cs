using System.Collections.Generic;
using System.Diagnostics;
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
            var parser = ArgumentParser.Build()
                .Recognize(new ArgumentLongname("argument", "a"))
                .Parse(new[] { "-a" });
            var arguments = parser.RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.ArgumentRecognizer.ArgumentLongname.Value, Is.EqualTo("argument"));
        }

        [Test]
        public void Given_several_arguments_Then_the_correct_one_is_recognized()
        {
            var arguments = ArgumentParser.Build()
                .Recognize(new ArgumentLongname("beta", "b"))
                .Parse(new[] { "-a", "-b" }).RecognizedArguments;

            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument, Is.EqualTo("-b"));
        }

        [Test]
        public void Recognizes_longform()
        {
            var arguments = ArgumentParser.Build()
                .Recognize("beta")
                .Parse(new[] { "-a", "--beta" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument, Is.EqualTo("--beta"));
        }

        [Test]
        public void It_can_parse_parameter_value()
        {
            var arguments = ArgumentParser.Build()
                .Recognize("beta")
                .Parse(new[] { "-a", "--beta", "value" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument, Is.EqualTo("--beta"));
            Assert.That(arg1.Value, Is.EqualTo("value"));
        }

        [Test]
        public void It_can_report_unrecognized_parameters()
        {
            var arguments = ArgumentParser.Build()
               .Recognize("beta")
               .Parse(new[] { "-a", "--beta" }).UnRecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Value, Is.EqualTo("-a"));
        }
    }
}