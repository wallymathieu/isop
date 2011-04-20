﻿using System;
using System.Linq;
using Helpers.Console;
using NUnit.Framework;

namespace Helpers.Tests
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
                .Recognize("&argument")
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
                .Recognize("&beta")
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
               .Parse(new[] { "-a", "value", "--beta" }).UnRecognizedArguments;

            Assert.That(arguments.Select(arg => arg.Value), Is.EquivalentTo(new[] { "-a", "value" }));
        }
        [Test]
        public void It_wont_report_matched_parameters()
        {
            var arguments = ArgumentParser.Build()
                .Recognize("beta")
                .Parse(new[] { "--beta", "value" }).UnRecognizedArguments;

            Assert.That(arguments.Count(),Is.EqualTo(0));
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
        [Test]
        public void It_can_write_a_message_about_unrecognized_parameters()
        {
            var val = ArgumentParser.Build()
                .Recognize("beta")
                .Parse(new[] { "-a", "value", "--beta" }).UnRecognizedArgumentsMessage();

            Assert.That(val, Is.StringContaining("-a"));
            Assert.That(val, Is.StringContaining("value"));
            Assert.That(val, Is.StringContaining("beta"));
        }
        [Test]
        public void It_can_invoke_recognized()
        {
            var count = 0;
            ArgumentParser.Build()
                           .RecognizeAction("beta", arg => count++)
                           .RecognizeAction("alpha", arg => Assert.Fail())
                           .Parse(new[] { "-a", "value", "--beta" }).Invoke();
            Assert.That(count, Is.EqualTo(1));
        }
    }
}