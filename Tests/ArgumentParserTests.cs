using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Isop.Parse;
using Isop.Parse.Parameters;
using Isop.Tests.FakeControllers;
using NUnit.Framework;

namespace Isop.Tests
{
    [TestFixture]
    public class ArgumentParserTests
    {
        [Test]
        public void Recognizes_shortform()
        {
            var parser = new Build()
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
            var arguments = new Build()
                .Parameter("&beta")
                .Parse(new[] { "-a", "-b" }).RecognizedArguments;

            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument, Is.EqualTo("b"));
        }

        [Test]
        public void Recognizes_longform()
        {
            var arguments = new Build()
                .Parameter("beta")
                .Parse(new[] { "-a", "--beta" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument, Is.EqualTo("beta"));
        }

        [Test]
        public void It_can_parse_parameter_value()
        {
            var arguments = new Build()
                .Parameter("beta")
                .Parse(new[] { "-a", "--beta", "value" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument, Is.EqualTo("beta"));
            Assert.That(arg1.Value, Is.EqualTo("value"));
        }
        [Test]
        public void It_can_parse_ordinalparameters()
        {
            ArgumentParameter o;
            Assert.That(OrdinalParameter.TryParse("#1first", CultureInfo.InvariantCulture, out o));
        }
        [Test]
        public void It_can_parse_ordinal_parameter_value()
        {
            var arguments = new Build()
                .Parameter("#0first")
                .Parse(new[] { "first" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument, Is.EqualTo("first"));
        }
        [Test]
        public void It_can_parse_parameter_with_equals()
        {
            var arguments = new Build()
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
            var arguments = new Build()
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
            var unRecognizedArguments = new Build()
               .Parameter("beta")
               .Parse(new[] { "-a", "value", "--beta" }).UnRecognizedArguments;

            Assert.That(unRecognizedArguments, Is.EquivalentTo(new[] {
                new UnrecognizedArgument {Index = 0,Value = "-a"},
                new UnrecognizedArgument {Index = 1,Value = "value" }
            }));
        }
        [Test]
        public void It_can_infer_ordinal_usage_of_named_parameters()
        {
            var arguments = new Build()
                .Parameter("beta|b=")
                .Parameter("alpha|a=")
                .Parse(new[] { "test", "value" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(2));
            var arg1 = arguments.First();
            Assert.That(arg1.WithOptions.Argument.ToString(), Is.EqualTo("beta|b="));
            Assert.That(arg1.Value, Is.EqualTo("test"));
            var arg2 = arguments.Last();
            Assert.That(arg2.WithOptions.Argument.ToString(), Is.EqualTo("alpha|a="));
            Assert.That(arg2.Value, Is.EqualTo("value"));
        }
        [Test]
        public void It_wont_report_matched_parameters()
        {
            var arguments = new Build()
                .Parameter("beta")
                .Parse(new[] { "--beta", "value" }).UnRecognizedArguments;

            Assert.That(arguments.Count(), Is.EqualTo(0));
        }
        [Test]
        public void It_will_fail_if_argument_not_supplied_and_it_is_required()
        {
            Assert.Throws<MissingArgumentException>(() => new Build()
               .Parameter("beta", required: true)
               .Parse(new[] { "-a", "value" }));

        }
        [Test]
        public void It_can_recognize_arguments()
        {
            var arguments = new Build()
                .Parameter("alpha")
                .Parse(new[] { "alpha" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.WithOptions.Argument.ToString(), Is.EqualTo("alpha"));
            Assert.That(arg1.Value, Is.Null);
            Assert.That(arg1.Argument, Is.EqualTo("alpha"));
        }

        [Test]
        public void It_can_parse_class_and_method_and_execute()
        {
            var count = 0;
            Func<Type, object> factory = (Type t) =>
                                            {
                                                Assert.That(t, Is.EqualTo(typeof(MyController)));
                                                return
                                                    (object)
                                                    new MyController() { OnAction = (p1, p2, p3, p4) => (count++).ToString() };
                                            };
            var arguments = new Build()
                                               .SetCulture(CultureInfo.InvariantCulture)
                                               .Recognize(typeof(MyController))
                                               .SetFactory(factory)
                                               .Parse(new[] { "My", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void It_can_parse_class_and_method_and_execute_with_ordinal_syntax()
        {
            var count = 0;
            Func<Type, object> factory = (Type t) =>
            {
                Assert.That(t, Is.EqualTo(typeof(MyController)));
                return
                    (object)
                    new MyController() { OnAction = (p1, p2, p3, p4) => (count++).ToString() };
            };
            var arguments = new Build()
                                               .SetCulture(CultureInfo.InvariantCulture)
                                               .Recognize(typeof(MyController))
                                               .SetFactory(factory)
                                               .Parse(new[] { "My", "Action",  "value1","value2", "3", "3.4" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void It_can_parse_class_and_method_and_knows_whats_required()
        {
            Func<Type, object> factory = (Type t) =>
            {
                Assert.That(t, Is.EqualTo(typeof(MyController)));
                return
                    (object)
                    new MyController() { OnAction = (p1, p2, p3, p4) => "" };
            };
            var build = new Build()
                            .SetCulture(CultureInfo.InvariantCulture)
                            .Recognize(typeof(MyController))
                            .SetFactory(factory);
            var expected = DictionaryDescriptionToKv("[param1, True], [param2, True], [param3, True], [param4, True]",Boolean.Parse);

            var recognizers = build.ControllerRecognizers.Single().Value().GetRecognizers("Action");
            Assert.That(recognizers.Select(r => new KeyValuePair<string, bool>(r.Argument.Prototype, r.Required)).ToArray(),
                Is.EquivalentTo(expected.ToArray()));
        }

        private static IEnumerable<KeyValuePair<string, T>> DictionaryDescriptionToKv<T>(string input, Func<string,T> convert)
        {
            var expected = Regex.Matches(input,
                                         @"\[(?<p>#?\w*), (?<v>\w*)\]")
                .Cast<Match>()
                .Select(m => new KeyValuePair<string, T>(m.Groups["p"].Value, convert(m.Groups["v"].Value)));
            return expected;
        }

        [Test]
        public void It_can_parse_class_and_method_and_knows_whats_not_required()
        {
            Func<Type, object> factory = (Type t) =>
            {
                Assert.That(t, Is.EqualTo(typeof(MyOptionalController)));
                return
                    (object)
                    new MyOptionalController() { OnAction = (p1, p2, p3, p4) => "" };
            };
            var build = new Build()
                            .SetCulture(CultureInfo.InvariantCulture)
                            .Recognize(typeof(MyOptionalController))
                            .SetFactory(factory);
            var expected = DictionaryDescriptionToKv("[param1, True], [param2, False], [param3, False], [param4, False]",Boolean.Parse);

            var recognizers = build.ControllerRecognizers.Single().Value().GetRecognizers("Action");
            Assert.That (recognizers.Select(r => new KeyValuePair<string, bool>(r.Argument.Prototype, r.Required)).ToArray(),
                Is.EquivalentTo(expected.ToArray()));
        }

        [Test]
        public void It_can_parse_class_and_method_and_executes_default_with_the_default_values()
        {
            var parameters = new object[0];
            Func<Type, object> factory = (Type t) =>
            {
                Assert.That(t, Is.EqualTo(typeof(MyOptionalController)));
                return
                    (object)
                    new MyOptionalController() { OnAction = (p1, p2, p3, p4) =>
                                                                { parameters = new object[] { p1, p2, p3, p4 }; return ""; }
                                               };
            };
            var arguments = new Build()
                    .SetCulture(CultureInfo.InvariantCulture)
                    .Recognize(typeof(MyOptionalController))
                    .SetFactory(factory)
                    .Parse(new[] { "MyOptional", "Action", "--param1", "value1"});
            arguments.Invoke(new StringWriter());
            Assert.That(parameters, Is.EquivalentTo(new object[]{"value1",null,null,1}));
        }

        [Test]
        public void It_can_parse_class_and_method_and_executes_default_with_the_default_values_when_using_ordinal_syntax()
        {
            var parameters = new object[0];
            Func<Type, object> factory = (Type t) =>
            {
                Assert.That(t, Is.EqualTo(typeof(MyOptionalController)));
                return
                    (object)
                    new MyOptionalController()
                    {
                        OnAction = (p1, p2, p3, p4) =>
                        { parameters = new object[] { p1, p2, p3, p4 }; return ""; }
                    };
            };
            var arguments = new Build()
                    .SetCulture(CultureInfo.InvariantCulture)
                    .Recognize(typeof(MyOptionalController))
                    .SetFactory(factory)
                    .Parse(new[] { "MyOptional", "Action", "value1" });
            arguments.Invoke(new StringWriter());
            Assert.That(parameters, Is.EquivalentTo(new object[] { "value1", null, null, 1 }));
        }


        [Test]
        public void It_can_parse_class_and_method_and_fail()
        {
            var builder = new Build()
                .SetCulture(CultureInfo.InvariantCulture)
                .Recognize(typeof(MyController));

            Assert.Throws<MissingArgumentException>(() => builder.Parse(new[] { "My", "Action", "--param2", "value2", "--paramX", "3", "--param1", "value1", "--param4", "3.4" }));
        }

        [Test]
        public void It_can_parse_class_and_method_and_fail_because_of_type_conversion()
        {
            var builder = new Build()
                 .SetCulture(CultureInfo.InvariantCulture)
                 .Recognize(typeof(SingleIntAction));
            Assert.Throws<TypeConversionFailedException>(() =>
                builder.Parse(new[] { "SingleIntAction", "Action", "--param", "value" })
            );
        }

        [Test]
        public void It_can_parse_class_and_method_and_fail_because_no_arguments_given()
        {
            var builder = new Build()
                .SetCulture(CultureInfo.InvariantCulture)
                .Recognize(typeof(MyController));

            Assert.Throws<MissingArgumentException>(() => builder.Parse(new[] { "My", "Action" }));
        }

        [Test]
        public void It_can_parse_class_and_method_and_also_arguments_and_execute()
        {
            var count = 0;
            var countArg = 0;
            Func<Type, object> factory = (Type t) =>
                                            {
                                                Assert.That(t, Is.EqualTo(typeof(MyController)));
                                                return
                                                    (object)
                                                    new MyController() { OnAction = (p1, p2, p3, p4) => (count++).ToString() };
                                            };
            var arguments = new Build()
                                               .SetCulture(CultureInfo.InvariantCulture)
                                               .Recognize(typeof(MyController))
                                               .Parameter("beta", arg => countArg++)
                                               .SetFactory(factory)
                                               .Parse(new[] { "My", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4", "--beta" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
            Assert.That(countArg, Is.EqualTo(1));
        }

        [Test]
        public void It_can_parse_class_and_default_method_and_execute()
        {
            var count = 0;
            Func<Type, object> factory = (Type t) =>
            {
                Assert.That(t, Is.EqualTo(typeof(WithIndexController)));
                return
                    (object)
                    new WithIndexController() { OnIndex = (p1, p2, p3, p4) => (count++).ToString() };
            };

            var arguments = new Build()
                                               .SetCulture(CultureInfo.InvariantCulture)
                                               .Recognize(typeof(WithIndexController))
                                               .SetFactory(factory)
                                               .Parse(new[] { "WithIndex", /*"Index", */"--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void It_can_invoke_recognized()
        {
            var count = 0;
            new Build()
                           .Parameter("beta", arg => count++)
                           .Parameter("alpha", arg => Assert.Fail())
                           .Parse(new[] { "-a", "value", "--beta" }).Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void It_understands_method_returning_enumerable()
        {
            var count = 0;
            var createCount = 0;
            Func<Type, object> factory = (Type t) =>
            {
                Assert.That(t, Is.EqualTo(typeof(EnumerableController)));
                createCount++;
                return
                    (object)
                    new EnumerableController() { Length = 2, OnEnumerate = () => (count++) };
            };

            var arguments = new Build()
                   .Recognize(typeof(EnumerableController))
                   .SetFactory(factory)
                   .Parse(new[] { "Enumerable", "Return" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(2));
            Assert.That(createCount, Is.EqualTo(1));
        }

        [Test]
        public void It_can_parse_class_and_method_with_object_and_execute()
        {
            var count = 0;
            Func<Type, object> factory = (Type t) =>
            {
                Assert.That(t, Is.EqualTo(typeof(MyObjectController)));
                return
                    (object)
                    new MyObjectController() { OnAction = (p1) => (count++).ToString() };
            };
            var arguments = new Build()
                                               .SetCulture(CultureInfo.InvariantCulture)
                                               .Recognize(typeof(MyObjectController))
                                               .SetFactory(factory)
                                               .Parse(new[] { "MyObject", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void It_can_handle_file_argument()
        {
            var filename = string.Empty;
            Func<Type, object> factory = (Type t) =>
                {
                    Assert.That(t, Is.EqualTo(typeof(MyFileController)));
                    return
                        (object)
                        new MyFileController()
                            {
                                OnAction = (file) => filename = file.Name
                            };
                };


            FileStream fileStream = null;
            try
            {

                var arguments = new Build()
                        .SetCulture(CultureInfo.InvariantCulture)
                        .SetTypeConverter((t, s, c) =>
                                              {

                                                  fileStream = new FileStream(s, FileMode.Create);
                                                  return fileStream;
                                              })
                    //Need to set type converter 
                        .Recognize(typeof(MyFileController))
                        .SetFactory(factory)
                        .Parse(new[] { "MyFile", "Action", "--file", "myfile.txt" });

                Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
                arguments.Invoke(new StringWriter());
                Assert.That(filename, Is.StringContaining("myfile.txt"));
            }
            finally
            {
                if (null != fileStream)
                {
                    try
                    {
                        fileStream.Dispose();
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch { }
                    // ReSharper restore EmptyGeneralCatchClause
                }
            }
        }
    }
}
