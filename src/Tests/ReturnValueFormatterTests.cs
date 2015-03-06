using System;
using NUnit.Framework;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Isop.Tests
{
    [TestFixture]
    public class ReturnValueFormatterTests
    {
        class WithTwoProperties
        {
            public WithTwoProperties(int value)
            {
                First = value;
                Second = "V"+value.ToString();
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


        [Test]
        public void It_can_format_object_as_table()
        {
            var count = 0;
            Func<Type, object> factory = (Type t) =>
                {
                    Assert.That(t, Is.EqualTo(typeof(ObjectController)));
                    return
                        (object)
                        new ObjectController() { OnAction = () => new WithTwoProperties(count++) };
                };
            var arguments = new Build { typeof(ObjectController) }
                .SetCulture(CultureInfo.InvariantCulture)
                .SetFactory(factory)
                .FormatObjectsAsTable()
                .Parse(new[] { "Object", "Action" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            var writer = new StringWriter();
            arguments.Invoke(writer);
            Assert.That(Split(writer.ToString()),Is.EquivalentTo(Split("First\tSecond\n0\tV0\n")));
        }

        [Test]
        public void It_can_format_ienumerable_objects_as_table()
        {
            var count = 0;
            Func<Type, object> factory = (Type t) =>
                {
                    Assert.That(t, Is.EqualTo(typeof(ObjectController)));
                    return
                        (object)
                        new ObjectController() { OnAction = () => new []{ new WithTwoProperties(count++), new WithTwoProperties(count++)} };
                };
            var arguments = new Build { typeof(ObjectController) }
                .SetCulture(CultureInfo.InvariantCulture)
                .SetFactory(factory)
                .FormatObjectsAsTable()
                .Parse(new[] { "Object", "Action" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            var writer = new StringWriter();
            arguments.Invoke(writer);
            Assert.That(Split(writer.ToString()),Is.EquivalentTo(Split("First\tSecond\n0\tV0\n1\tV1\n")));
        }
		private string[] Split(string value)
		{
			return value.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}

