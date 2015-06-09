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
using NUnit.Framework;

namespace Isop.Tests
{
    
    [TestFixture]
    public class DisposeTests
    {
        [Test]
        public void It_will_not_dispose_factory_created_instances_set_to_be_recognized()
        {
            var count = 0;
            Func<Type, object> factory = (Type t) =>
                {
                    Assert.That(t, Is.EqualTo(typeof(DisposeController)));
                    var c = new DisposeController();
                    c.OnDispose+= () => count++;
                    return c;
                };
            using (var build = new Build()
                .SetCulture(CultureInfo.InvariantCulture)
                .Recognize<DisposeController>()
                .SetFactory(factory))
            {
                build
                    .Parse(new[] { "Dispose", "method" })
                    .Invoke(new StringWriter());
            }
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void It_will_not_dispose_instances_set_to_be_recognized()
        {
            var count = 0;
            var c = new DisposeController();
            c.OnDispose+= () => count++;

            using (var build = new Build()
                .SetCulture(CultureInfo.InvariantCulture)
                .Recognize(c))
            {
                build
                    .Parse(new[] { "Dispose", "method" })
                    .Invoke(new StringWriter());
            }
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void It_will_dispose_types_created_by_default_factory_set_to_be_recognized()
        {
            var count = 0;
            DisposeController.StaticOnDispose += c => { count++; };

            using (var build = new Build()
                .SetCulture(CultureInfo.InvariantCulture)
                .Recognize<DisposeController>())
            {
                build
                    .Parse(new[] { "Dispose", "method" })
                    .Invoke(new StringWriter());
            }
            Assert.That(count, Is.EqualTo(1));
        }
    }
}
