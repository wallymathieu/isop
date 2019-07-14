using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isop;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tests.ArgumentParsers;

namespace Tests
{
    [TestFixture]
    public class Given_optional_controller_action_parameter
    {
        class MyOptionalController
        {
            public MyOptionalController()
            {
                OnAction = (p1, p2, p3, p4) => string.Empty;
            }
            public Func<string, string, int?, decimal, string> OnAction { get; set; }
            public string Action(string param1, string param2 = null, int? param3 = null, decimal param4 = 1) { return OnAction(param1, param2, param3, param4); }
        }       
        [Test]
        public void It_can_parse_class_and_method_and_knows_whats_not_required()
        {
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyOptionalController() { OnAction = (p1, p2, p3, p4) => "" });
            var build = Builder.Create(sc).Recognize(typeof(MyOptionalController)).BuildAppHost();
            var expected = Helpers.DictionaryDescriptionToKv("[param1, True], [param2, False], [param3, False], [param4, False]", Boolean.Parse);

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
            var arguments = Builder.Create(sc).Recognize<MyOptionalController>()
                .BuildAppHost()
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
            var arguments = Builder.Create(sc).Recognize<MyOptionalController>()
                .BuildAppHost()
                .Parse(new[] { "MyOptional", "Action", "value1" });
            arguments.Invoke(new StringWriter());
            Assert.That(parameters, Is.EquivalentTo(new object[] { "value1", null, null, 1 }));
        }

    }
}