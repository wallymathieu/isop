﻿using System;
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
using Tests.Routing;

namespace Tests
{
    [TestFixture]
    public class ArgumentParserTests
    {
        public class SingleIntAction
        {
            public void Action(int param) { }
        }
        
        [Test]
        public void Recognizes_shortform()
        {
            var parser = AppHostBuilder.Create()
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
            var arguments = AppHostBuilder.Create()
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
            var arguments = AppHostBuilder.Create()
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
            var arguments = AppHostBuilder.Create()
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
            var arguments = AppHostBuilder.Create()
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
            var arguments = AppHostBuilder.Create()
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
            var arguments = AppHostBuilder.Create()
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
            var unRecognizedArguments = AppHostBuilder.Create()
               .Parameter("beta")
               .BuildAppHost()
               .Parse(new[] { "-a", "value", "--beta" }).Unrecognized;

            Assert.That(unRecognizedArguments.Select(unrecognized=>Tuple.Create(unrecognized.Index,unrecognized.Value)), Is.EquivalentTo(new[] {
                Tuple.Create(0,"-a"),
                Tuple.Create(1,"value" )
            }));
        }
        [Test]
        public void It_can_infer_ordinal_usage_of_named_parameters()
        {
            var arguments = AppHostBuilder.Create()
                .Parameter("beta|b=")
                .Parameter("alpha|a=")
                .BuildAppHost()
                .Parse(new[] { "test", "value" }).Recognized;
            Assert.That(arguments.Count, Is.EqualTo(2));
            var arg1 = arguments.First();
            Assert.That(arg1.Value, Is.EqualTo("test"));
            var arg2 = arguments.Last();
            Assert.That(arg2.Value, Is.EqualTo("value"));
        }
        [Test]
        public void When_infer_is_turned_off()
        {
            var parsed = AppHostBuilder.Create(new Configuration
                {
                    DisableAllowInferParameter = true
                })
                .Parameter("beta|b=")
                .Parameter("alpha|a=")
                .BuildAppHost()
                .Parse(new[] { "test", "value" });
            Assert.That(parsed.Recognized.Count, Is.EqualTo(0));
            CollectionAssert.AreEqual(
                new []{"test", "value"},
                parsed.Unrecognized.Select(u=>u.Value));
        }
        [Test]
        public void It_wont_report_matched_parameters()
        {
            var arguments = AppHostBuilder.Create()
                .Parameter("beta")
                .BuildAppHost()
                .Parse(new[] { "--beta", "value" }).Unrecognized;

            Assert.That(arguments.Count(), Is.EqualTo(0));
        }
        [Test]
        public void It_will_fail_if_argument_not_supplied_and_it_is_required() =>
            Assert.Throws<MissingArgumentException>(() => AppHostBuilder.Create()
                .Parameter("beta", required: true)
                .BuildAppHost()
                .Parse(new[] { "-a", "value" }).Invoke(Console.Out));

        [Test]
        public void It_can_recognize_arguments()
        {
            var arguments = AppHostBuilder.Create()
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
            var builder = AppHostBuilder.Create().Recognize<SingleIntAction>()
                 .BuildAppHost();
            Assert.Throws<TypeConversionFailedException>(() =>
                builder.Parse(new[] { "SingleIntAction", "Action", "--param", "value" })
            );
        }

        [Test]
        public void It_can_invoke_recognized()
        {
            var count = 0;
            AppHostBuilder.Create()
                           .Parameter("beta", arg => count++)
                           .Parameter("alpha", arg => Assert.Fail())
                           .BuildAppHost()
                           .Parse(new[] { "-a", "value", "--beta" }).Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

    }
}
