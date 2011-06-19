﻿using System;
using System.Globalization;
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
                .Parameter("&argument")
                .Parse(new[] { "-a" });
            var arguments = parser.RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.WithOptions.Argument.ToString(), Is.EqualTo("&argument"));
        }

        [Test]
        public void Given_several_arguments_Then_the_correct_one_is_recognized()
        {
            var arguments = ArgumentParser.Build()
                .Parameter("&beta")
                .Parse(new[] { "-a", "-b" }).RecognizedArguments;

            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument, Is.EqualTo("b"));
        }

        [Test]
        public void Recognizes_longform()
        {
            var arguments = ArgumentParser.Build()
                .Parameter("beta")
                .Parse(new[] { "-a", "--beta" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument, Is.EqualTo("beta"));
        }

        [Test]
        public void It_can_parse_parameter_value()
        {
            var arguments = ArgumentParser.Build()
                .Parameter("beta")
                .Parse(new[] { "-a", "--beta", "value" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument, Is.EqualTo("beta"));
            Assert.That(arg1.Value, Is.EqualTo("value"));
        }
        [Test]
        public void It_can_parse_parameter_with_equals()
        {
            var arguments = ArgumentParser.Build()
                .Parameter("beta=")
                .Parse(new[] { "-a", "--beta=test", "value" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Value, Is.EqualTo("test"));
            Assert.That(arg1.Argument, Is.EqualTo("beta"));
        }
        [Test]
        public void It_can_parse_parameter_alias()
        {
            var arguments = ArgumentParser.Build()
                .Parameter("beta|b=")
                .Parse(new[] { "-a", "-b=test", "value" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.WithOptions.Argument.ToString(), Is.EqualTo("beta|b="));
            Assert.That(arg1.Value, Is.EqualTo("test"));
            Assert.That(arg1.Argument, Is.EqualTo("b"));
        }
        [Test]
        public void It_can_report_unrecognized_parameters()
        {
            var unRecognizedArguments = ArgumentParser.Build()
               .Parameter("beta")
               .Parse(new[] { "-a", "value", "--beta" }).UnRecognizedArguments;

            Assert.That(unRecognizedArguments, Is.EquivalentTo(new[] {
                new UnrecognizedArgument {Index = 0,Value = "-a"},
                new UnrecognizedArgument {Index = 1,Value = "value" }
            }));
        }
        [Test]
        public void It_wont_report_matched_parameters()
        {
            var arguments = ArgumentParser.Build()
                .Parameter("beta")
                .Parse(new[] { "--beta", "value" }).UnRecognizedArguments;

            Assert.That(arguments.Count(), Is.EqualTo(0));
        }
        [Test]
        public void It_will_fail_if_argument_not_supplied_and_it_is_required()
        {
            Assert.Throws<MissingArgumentException>(() => ArgumentParser.Build()
               .Parameter("beta", required: true)
               .Parse(new[] { "-a", "value" }));

        }
        [Test]
        public void It_can_recognize_arguments()
        {
            var arguments = ArgumentParser.Build()
                .Argument("alpha")
                .Parse(new[] { "alpha" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.WithOptions.Argument.ToString(), Is.EqualTo("alpha"));
            Assert.That(arg1.Value, Is.Null);
            Assert.That(arg1.Argument, Is.EqualTo("alpha"));
        }

        [Test]
        public void It_can_parse_class_and_method()
        {
            var arguments = (ParsedMethod)ArgumentParser.Build()
                                               .SetCulture(CultureInfo.InvariantCulture)
                                               .Recognize(typeof(MyController))
                                               .Parse(new[] { "My", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4" });
            Assert.That(arguments.RecognizedAction.Name, Is.EqualTo("Action"));
            Assert.That(arguments.RecognizedActionParameters, Is.EquivalentTo(new object[] { "value1", "value2", 3, 3.4m }));
            Assert.That(!arguments.UnRecognizedArguments.Any());
        }
        [Test]
        public void It_can_parse_class_and_method_and_execute()
        {
            var count = 0;
            var arguments = (ParsedMethod)ArgumentParser.Build()
                                               .SetCulture(CultureInfo.InvariantCulture)
                                               .Recognize(typeof(MyController))
                                               .Parse(new[] { "My", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4" });
            Func<Type, object> factory = (Type t) =>
                                             {
                                                 Assert.That(t, Is.EqualTo(typeof(MyController)));
                                                 return
                                                     (object)
                                                     new MyController() { OnAction = (p1, p2, p3, p4) => (count++).ToString() };
                                             };
            arguments.Invoke(factory);
            Assert.That(count, Is.EqualTo(count));
        }
        private class MyController
        {
            public MyController()
            {
                OnAction = (p1, p2, p3, p4) => string.Empty;
            }
            public Func<string, string, int, decimal, string> OnAction { get; set; }
            public string Action(string param1, string param2, int param3, decimal param4) { return OnAction(param1, param2, param3, param4); }
        }

        [Test]
        public void It_can_invoke_recognized()
        {
            var count = 0;
            ArgumentParser.Build()
                           .Parameter("beta", arg => count++)
                           .Parameter("alpha", arg => Assert.Fail())
                           .Parse(new[] { "-a", "value", "--beta" }).Invoke();
            Assert.That(count, Is.EqualTo(1));
        }
    }
}