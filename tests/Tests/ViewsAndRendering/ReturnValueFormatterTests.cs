using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Isop;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests.ViewsAndRendering
{
    [TestFixture]
    public class ReturnValueFormatterTests
    {
        
        class WithTwoProperties
        {
            public WithTwoProperties(int value)
            {
                First = value;
                Second = "V" + value.ToString();
            }
            public int First
            {
                get;
                set;
            }
            public string Second
            {
                get;
                set;
            }
        }
        public class ObjectController
        {
            public ObjectController()
            {
                OnAction = () => string.Empty;
            }
            public Func<object> OnAction { get; set; }
            /// <summary>
            /// ActionHelp
            /// </summary>
            public object Action() { return OnAction(); }
        }

        [Test]
        public async Task It_can_format_object_as_table()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new ObjectController() { OnAction = () => new WithTwoProperties(count++) });

            var arguments = AppHostBuilder.Create(sc, new AppHostConfiguration
            {
                CultureInfo = CultureInfo.InvariantCulture
            }).Recognize( typeof(ObjectController) )
                .FormatObjectsAsTable()
                .BuildAppHost()
                .Parse(new[] { "Object", "Action" });

            Assert.That(arguments.Unrecognized.Count(), Is.EqualTo(0));
            var writer = new StringWriter();
            await arguments.InvokeAsync(writer);
            Assert.That(Split(writer.ToString()), Is.EquivalentTo(Split("First\tSecond\n0\tV0\n")));
        }

        [Test]
        public async Task It_can_format_enumerable_objects_as_table()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new ObjectController { OnAction = () => new[] { new WithTwoProperties(count++), new WithTwoProperties(count++) } });

            var arguments = AppHostBuilder.Create(sc, new AppHostConfiguration
            {
                CultureInfo = CultureInfo.InvariantCulture
            }).Recognize(typeof(ObjectController))
                .FormatObjectsAsTable()
                .BuildAppHost()
                .Parse(new[] { "Object", "Action" });

            Assert.That(arguments.Unrecognized.Count(), Is.EqualTo(0));
            var writer = new StringWriter();
            await arguments.InvokeAsync(writer);
            Assert.That(Split(writer.ToString()), Is.EquivalentTo(Split("First\tSecond\n0\tV0\n1\tV1\n")));
        }
        private string[] Split(string value)
        {
            return value.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}

