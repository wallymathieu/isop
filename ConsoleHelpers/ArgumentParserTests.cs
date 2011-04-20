using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace ConsoleHelpers
{
    [TestFixture]
    internal class ArgumentParserTests
    {
        [SetUp]
        public void SetUp() { }
        [TearDown]
        public void TearDown() { }

        [Test]
        public void Recognizes_shortform()
        {
            var parser = ArgumentParser.Build()
                .Recognize(new ArgumentName("argument", "a"))
                .Parse(new[] { "-a" });
            var arguments = parser.RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Recognizer.ArgumentName.Value, Is.EqualTo("argument"));
        }

        [Test]
        public void Given_several_arguments_Then_the_correct_one_is_recognized()
        {
            var arguments = ArgumentParser.Build()
                .Recognize(new ArgumentName("beta", "b"))
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

        [Test]
        public void It_can_parse_class_and_method()
        {
            var arguments = ArgumentParser.Build()
                .Recognize(typeof(MyController))
                .Parse(new[] { "My", "Action", "--param2", "value2", "--param3", "value3", "--param1", "value1" });
            Assert.That(arguments.RecognizedAction.Name, Is.EqualTo("Action"));
            Assert.That(arguments.RecognizedActionParameters, Is.EquivalentTo(new object[] { "value1", "value2", "value3" }));
        }
        private class MyController
        {
            public string Action(string param1, string param2, string param3) { return ""; }
        }
    }
}