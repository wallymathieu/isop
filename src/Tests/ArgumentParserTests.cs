using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Isop.CommandLine.Parse;
using Isop.CommandLine.Parse.Parameters;
using Isop.Tests.FakeControllers;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Isop.Tests
{
    [TestFixture]
    public class ArgumentParserTests
    {
        [Test]
        public void Recognizes_shortform()
        {
            var parser = Build.Create()
                .Parameter("&argument")
                .Build()
                .Parse(new[] { "-a" });
            var arguments = parser.RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Argument.Name, Is.EqualTo("argument"));
        }

        [Test]
        public void Given_several_arguments_Then_the_correct_one_is_recognized()
        {
            var arguments = Build.Create()
                .Parameter("&beta")
                .Build()
                .Parse(new[] { "-a", "-b" }).RecognizedArguments;

            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.RawArgument, Is.EqualTo("b"));
        }

        [Test]
        public void Recognizes_longform()
        {
            var arguments = Build.Create()
                .Parameter("beta")
                .Build()
                .Parse(new[] { "-a", "--beta" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.RawArgument, Is.EqualTo("beta"));
        }

        [Test]
        public void It_can_parse_parameter_value()
        {
            var arguments = Build.Create()
                .Parameter("beta")
                .Build()
                .Parse(new[] { "-a", "--beta", "value" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.RawArgument, Is.EqualTo("beta"));
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
            var arguments = Build.Create()
                .Parameter("#0first")
                .Build()
                .Parse(new[] { "first" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.RawArgument, Is.EqualTo("first"));
        }
        [Test]
        public void It_can_parse_parameter_with_equals()
        {
            var arguments = Build.Create()
                .Parameter("beta=")
                .Build()
                .Parse(new[] { "-a", "--beta=test", "value" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Value, Is.EqualTo("test"));
            Assert.That(arg1.RawArgument, Is.EqualTo("beta"));
        }
        [Test]
        public void It_can_parse_parameter_alias()
        {
            var arguments = Build.Create()
                .Parameter("beta|b=")
                .Build()
                .Parse(new[] { "-a", "-b=test", "value" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Value, Is.EqualTo("test"));
            Assert.That(arg1.RawArgument, Is.EqualTo("b"));
        }
        [Test]
        public void It_can_report_unrecognized_parameters()
        {
            var unRecognizedArguments = Build.Create()
               .Parameter("beta")
               .Build()
               .Parse(new[] { "-a", "value", "--beta" }).UnRecognizedArguments;

            Assert.That(unRecognizedArguments, Is.EquivalentTo(new[] {
                new UnrecognizedArgument {Index = 0,Value = "-a"},
                new UnrecognizedArgument {Index = 1,Value = "value" }
            }));
        }
        [Test]
        public void It_can_infer_ordinal_usage_of_named_parameters()
        {
            var arguments = Build.Create()
                .Parameter("beta|b=")
                .Parameter("alpha|a=")
                .Build()
                .Parse(new[] { "test", "value" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(2));
            var arg1 = arguments.First();
            Assert.That(arg1.Value, Is.EqualTo("test"));
            var arg2 = arguments.Last();
            Assert.That(arg2.Value, Is.EqualTo("value"));
        }
        [Test]
        public void It_wont_report_matched_parameters()
        {
            var arguments = Build.Create()
                .Parameter("beta")
                .Build()
                .Parse(new[] { "--beta", "value" }).UnRecognizedArguments;

            Assert.That(arguments.Count(), Is.EqualTo(0));
        }
        [Test]
        public void It_will_fail_if_argument_not_supplied_and_it_is_required()
        {
            Assert.Throws<MissingArgumentException>(() => Build.Create()
               .Parameter("beta", required: true)
               .Build()
               .Parse(new[] { "-a", "value" }));

        }
        [Test]
        public void It_can_recognize_arguments()
        {
            var arguments = Build.Create()
                .Parameter("alpha")
                .Build()
                .Parse(new[] { "alpha" }).RecognizedArguments;
            Assert.That(arguments.Count(), Is.EqualTo(1));
            var arg1 = arguments.First();
            Assert.That(arg1.Value, Is.Null);
            Assert.That(arg1.RawArgument, Is.EqualTo("alpha"));
        }

        [Test]
        public void It_can_parse_class_and_method_and_execute()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyController() { OnAction = (p1, p2, p3, p4) => (count++).ToString() });

            var arguments = Build.Create(sc).Recognize<MyController>()
                                .Build()
                                .Parse(new[] { "My", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void It_can_parse_class_and_method_and_execute_with_ordinal_syntax()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyController() { OnAction = (p1, p2, p3, p4) => (count++).ToString() });
            var arguments = Build.Create(sc).Recognize<MyController>()
                            .Build()
                            .Parse(new[] { "My", "Action", "value1", "value2", "3", "3.4" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void It_can_parse_class_and_method_and_knows_whats_required()
        {
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyController() { OnAction = (p1, p2, p3, p4) => "" });
            var build = Build.Create(sc).Recognize<MyController>()
                            .Build();
            var expected = DictionaryDescriptionToKv("[param1, True], [param2, True], [param3, True], [param4, True]", Boolean.Parse);

            var recognizers = build.Controller("My").Action("Action").GetArguments();
            Assert.That(recognizers.Select(r => new KeyValuePair<string, bool>(r.Name, r.Required)).ToArray(),
                Is.EquivalentTo(expected.ToArray()));
        }

        private static IEnumerable<KeyValuePair<string, T>> DictionaryDescriptionToKv<T>(string input, Func<string, T> convert)
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
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyOptionalController() { OnAction = (p1, p2, p3, p4) => "" });
            var build = Build.Create(sc).Recognize(typeof(MyOptionalController)).Build();
            var expected = DictionaryDescriptionToKv("[param1, True], [param2, False], [param3, False], [param4, False]", Boolean.Parse);

            var recognizers = build.Controller("MyOptional").Action("Action").GetArguments();
            Assert.That(recognizers.Select(r => new KeyValuePair<string, bool>(r.Name, r.Required)).ToArray(),
                Is.EquivalentTo(expected.ToArray()));
        }

        [Test]
        public void It_can_parse_class_and_method_and_executes_default_with_the_default_values()
        {
            var parameters = new object[0];
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyOptionalController { OnAction = (p1, p2, p3, p4) =>
                { parameters = new object[] { p1, p2, p3, p4 }; return ""; } });
            var arguments = Build.Create(sc).Recognize<MyOptionalController>()
                    .Build()
                    .Parse(new[] { "MyOptional", "Action", "--param1", "value1" });
            arguments.Invoke(new StringWriter());
            Assert.That(parameters, Is.EquivalentTo(new object[] { "value1", null, null, 1 }));
        }

        [Test]
        public void It_can_parse_class_and_method_and_executes_default_with_the_default_values_when_using_ordinal_syntax()
        {
            var parameters = new object[0];
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyOptionalController { OnAction = (p1, p2, p3, p4) =>
                { parameters = new object[] { p1, p2, p3, p4 }; return ""; } });
            var arguments = Build.Create(sc).Recognize<MyOptionalController>()
                    .Build()
                    .Parse(new[] { "MyOptional", "Action", "value1" });
            arguments.Invoke(new StringWriter());
            Assert.That(parameters, Is.EquivalentTo(new object[] { "value1", null, null, 1 }));
        }


        [Test]
        public void It_can_parse_class_and_method_and_fail()
        {
            var builder = Build.Create().Recognize<MyController>()
                .Build();

            Assert.Throws<MissingArgumentException>(() => builder.Parse(new[] { "My", "Action", "--param2", "value2", "--paramX", "3", "--param1", "value1", "--param4", "3.4" }));
        }

        [Test]
        public void It_can_parse_class_and_method_and_fail_because_of_type_conversion()
        {
            var builder = Build.Create().Recognize<SingleIntAction>()
                 .Build();
            Assert.Throws<TypeConversionFailedException>(() =>
                builder.Parse(new[] { "SingleIntAction", "Action", "--param", "value" })
            );
        }

        [Test]
        public void It_can_parse_class_and_method_and_fail_because_no_arguments_given()
        {
            var builder = Build.Create().Recognize<MyController>()
                .Build();

            Assert.Throws<MissingArgumentException>(() => builder.Parse(new[] { "My", "Action" }));
        }

        [Test]
        public void It_can_parse_class_and_method_and_also_arguments_and_execute()
        {
            var count = 0;
            var countArg = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyController() { OnAction = (p1, p2, p3, p4) => (count++).ToString() });

            var arguments = Build.Create(sc)
                        .Recognize(typeof(MyController))
                        .Parameter("beta", arg => countArg++)
                        .Build()
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
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new WithIndexController() { OnIndex = (p1, p2, p3, p4) => (count++).ToString() });

            var arguments = Build.Create(sc)
                                .Recognize(typeof(WithIndexController))
                                .Build()
                                .Parse(new[] { "WithIndex", /*"Index", */"--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void It_can_invoke_recognized()
        {
            var count = 0;
            Build.Create()
                           .Parameter("beta", arg => count++)
                           .Parameter("alpha", arg => Assert.Fail())
                           .Build()
                           .Parse(new[] { "-a", "value", "--beta" }).Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void It_understands_method_returning_enumerable()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new EnumerableController() { Length = 2, OnEnumerate = () => (count++) });

            var arguments = Build.Create(sc)
                   .Recognize(typeof(EnumerableController))
                   .Build()
                   .Parse(new[] { "Enumerable", "Return" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void It_can_parse_class_and_method_with_object_and_execute()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyObjectController() { OnAction = (p1) => (count++).ToString() });

            var arguments = Build.Create(sc)
                                               .Recognize(typeof(MyObjectController))
                                               .Build()
                                               .Parse(new[] { "MyObject", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void It_can_handle_file_argument()
        {
            var filename = string.Empty;

            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyFileController() { OnAction = (file) => filename = file.Name });


            FileStream fileStream = null;
            try
            {

                var arguments = Build.Create(sc)
                        
                        .SetTypeConverter((t, s, c) =>
                                              {

                                                  fileStream = new FileStream(s, FileMode.Create);
                                                  return fileStream;
                                              })
                        //Need to set type converter 
                        .Recognize(typeof(MyFileController))
                        .Build()
                        .Parse(new[] { "MyFile", "Action", "--file", "myfile.txt" });

                Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
                arguments.Invoke(new StringWriter());
                Assert.True(filename.Contains("myfile.txt"));
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
                    OnIndex = (p1) =>
                    {
                        parameters.Add(p1);
                        return "";
                    }
                });

                var arguments = Build.Create(sc)
                                                   .Recognize(typeof(WithEnumController))
                                                   .Build()
                                                   .Parse(new[] { "WithEnum", /*"Index", */"--value", pair.value });

                Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
                arguments.Invoke(new StringWriter());
                Assert.That(parameters, Is.EquivalentTo(new[] { pair.expected }));

            }
        }
    }
}
