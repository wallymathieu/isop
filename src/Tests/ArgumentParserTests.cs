using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Isop;
using Isop.CommandLine.Parse;
using Isop.CommandLine.Parse.Parameters;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tests.FakeControllers;

namespace Tests
{
    [TestFixture]
    public class ArgumentParserTests
    {
        [Test]
        public void Recognizes_shortform()
        {
            var parser = Builder.Create()
                .Parameter("&argument")
                .BuildAppHost()
                .Parse(new[] { "-a" });
            var arguments = parser.Recognized;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument.Name, Is.EqualTo("argument"));
        }

        [Test]
        public void Given_several_arguments_Then_the_correct_one_is_recognized()
        {
            var arguments = Builder.Create()
                .Parameter("&beta")
                .BuildAppHost()
                .Parse(new[] { "-a", "-b" }).Recognized;

            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.RawArgument, Is.EqualTo("b"));
        }

        [Test]
        public void Recognizes_longform()
        {
            var arguments = Builder.Create()
                .Parameter("beta")
                .BuildAppHost()
                .Parse(new[] { "-a", "--beta" }).Recognized;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.RawArgument, Is.EqualTo("beta"));
        }

        [Test]
        public void It_can_parse_parameter_value()
        {
            var arguments = Builder.Create()
                .Parameter("beta")
                .BuildAppHost()
                .Parse(new[] { "-a", "--beta", "value" }).Recognized;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.RawArgument, Is.EqualTo("beta"));
            Assert.That(arg1.Value, Is.EqualTo("value"));
        }
        [Test]
        public void It_can_parse_ordinal_parameters()
        {
            ArgumentParameter o;
            Assert.That(OrdinalParameter.TryParse("#1first", CultureInfo.InvariantCulture, out o));
        }
        [Test]
        public void It_can_parse_ordinal_parameter_value()
        {
            var arguments = Builder.Create()
                .Parameter("#0first")
                .BuildAppHost()
                .Parse(new[] { "first" }).Recognized;
            Assert.That(arguments.Count, Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.RawArgument, Is.EqualTo("first"));
        }
        [Test]
        public void It_can_parse_parameter_with_equals()
        {
            var arguments = Builder.Create()
                .Parameter("beta=")
                .BuildAppHost()
                .Parse(new[] { "-a", "--beta=test", "value" }).Recognized;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Value, Is.EqualTo("test"));
            Assert.That(arg1.RawArgument, Is.EqualTo("beta"));
        }
        [Test]
        public void It_can_parse_parameter_alias()
        {
            var arguments = Builder.Create()
                .Parameter("beta|b=")
                .BuildAppHost()
                .Parse(new[] { "-a", "-b=test", "value" }).Recognized;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Value, Is.EqualTo("test"));
            Assert.That(arg1.RawArgument, Is.EqualTo("b"));
        }
        [Test]
        public void It_can_report_unrecognized_parameters()
        {
            var unRecognizedArguments = Builder.Create()
               .Parameter("beta")
               .BuildAppHost()
               .Parse(new[] { "-a", "value", "--beta" }).Unrecognized;

            Assert.That(unRecognizedArguments, Is.EquivalentTo(new[] {
                new UnrecognizedArgument(0,"-a"),
                new UnrecognizedArgument(1,"value" )
            }));
        }
        [Test]
        public void It_can_infer_ordinal_usage_of_named_parameters()
        {
            var arguments = Builder.Create()
                .Parameter("beta|b=")
                .Parameter("alpha|a=")
                .BuildAppHost()
                .Parse(new[] { "test", "value" }).Recognized;
            Assert.That(arguments.Count(), Is.EqualTo(2));
            var arg1 = arguments.First();
            Assert.That(arg1.Value, Is.EqualTo("test"));
            var arg2 = arguments.Last();
            Assert.That(arg2.Value, Is.EqualTo("value"));
        }
        [Test]
        public void It_wont_report_matched_parameters()
        {
            var arguments = Builder.Create()
                .Parameter("beta")
                .BuildAppHost()
                .Parse(new[] { "--beta", "value" }).Unrecognized;

            Assert.That(arguments.Count(), Is.EqualTo(0));
        }
        [Test]
        public void It_will_fail_if_argument_not_supplied_and_it_is_required() =>
            Assert.Throws<MissingArgumentException>(() => Builder.Create()
                .Parameter("beta", required: true)
                .BuildAppHost()
                .Parse(new[] { "-a", "value" }).Invoke(Console.Out));

        [Test]
        public void It_can_recognize_arguments()
        {
            var arguments = Builder.Create()
                .Parameter("alpha")
                .BuildAppHost()
                .Parse(new[] { "alpha" }).Recognized;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Value, Is.Null);
            Assert.That(arg1.RawArgument, Is.EqualTo("alpha"));
        }

        [Test]
        public void It_can_parse_class_and_method_and_fail_because_of_type_conversion()
        {
            var builder = Builder.Create().Recognize<SingleIntAction>()
                 .BuildAppHost();
            Assert.Throws<TypeConversionFailedException>(() =>
                builder.Parse(new[] { "SingleIntAction", "Action", "--param", "value" })
            );
        }

        [Test]
        public void It_can_parse_class_and_default_method_and_execute()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new WithIndexController { OnIndex = (p1, p2, p3, p4) => (count++).ToString() });

            var arguments = Builder.Create(sc)
                                .Recognize(typeof(WithIndexController))
                                .BuildAppHost()
                                .Parse(new[] { "WithIndex", /*"Index", */"--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4" });

            Assert.That(arguments.Unrecognized.Select(u=>u.Value), Is.Empty);
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void It_can_invoke_recognized()
        {
            var count = 0;
            Builder.Create()
                           .Parameter("beta", arg => count++)
                           .Parameter("alpha", arg => Assert.Fail())
                           .BuildAppHost()
                           .Parse(new[] { "-a", "value", "--beta" }).Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void It_can_handle_different_casing_for_enum()
        {
            foreach (var pair in new[] {
                new { value = "param1", expected = WithEnumController.WithEnum.Param1 },
                new { value = "paramwithcasing", expected = WithEnumController.WithEnum.ParamWithCasing },
            })
            {
                var parameters = new List<WithEnumController.WithEnum?>();
                var sc = new ServiceCollection();
                sc.AddSingleton(ci => new WithEnumController
                {
                    OnIndex = p1 =>
                    {
                        parameters.Add(p1);
                        return "";
                    }
                });

                var arguments = Builder.Create(sc)
                                                   .Recognize(typeof(WithEnumController))
                                                   .BuildAppHost()
                                                   .Parse(new[] { "WithEnum", /*"Index", */"--value", pair.value });

                Assert.That(arguments.Unrecognized.Select(u=>u.Value), Is.Empty);
                arguments.Invoke(new StringWriter());
                Assert.That(parameters, Is.EquivalentTo(new[] { pair.expected }));
            }
        }
    }
}
