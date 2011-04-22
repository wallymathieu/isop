using System;
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
            Assert.That(arg1.Recognizer.Argument.Value, Is.EqualTo("argument"));
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
        public void It_will_fail_if_argument_not_supplied_and_it_is_required()
        {
            Assert.Throws<MissingArgumentException>(()=> ArgumentParser.Build()
               .Recognize("beta",required:true)
               .Parse(new[] { "-a", "value" }));

        }

        [Test]
        public void It_can_parse_class_and_method()
        {
            var arguments = ArgumentParser.Build()
                .Recognize(typeof(MyController))
                .ParseMethod(new[] { "My", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3,4" });
            Assert.That(arguments.RecognizedAction.Name, Is.EqualTo("Action"));
            Assert.That(arguments.RecognizedActionParameters, Is.EquivalentTo(new object[] { "value1", "value2", 3, 3.4m}));
        }
        [Test]
        public void It_can_parse_class_and_method_and_execute()
        {
            var count = 0;
            var arguments = ArgumentParser.Build()
              .Recognize(typeof(MyController))
              .ParseMethod(new[] { "My", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3,4" });
            Func<Type, object> factory = (Type t) =>
                                             {
                                                 Assert.That(t, Is.EqualTo(typeof(MyController)));
                                                 return
                                                     (object)
                                                     new MyController()
                                                         {OnAction = (p1, p2, p3, p4) => (count++).ToString()};
                                             };
            arguments.Invoke(factory);
            Assert.That(count, Is.EqualTo(count));
        }
        private class MyController
        {
            public MyController()
            {
                OnAction = (p1,p2,p3,p4) => string.Empty;
            }
            public Func<string,string,int,decimal,string> OnAction { get; set; }
            public string Action(string param1, string param2, int param3, decimal param4) { return OnAction(param1,param2,param3,param4); }
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
                           .Action("beta", arg => count++)
                           .Action("alpha", arg => Assert.Fail())
                           .Parse(new[] { "-a", "value", "--beta" }).Invoke();
            Assert.That(count, Is.EqualTo(1));
        }
    }
}