using System;
using System.Globalization;
using System.IO;
using Isop.Tests.FakeControllers;
using NUnit.Framework;

namespace Isop.Tests
{

    //[TestFixture]
    public class DisposeTests
    {
        [Test]
        public void It_will_not_dispose_instances_set_to_be_recognized()
        {
            var count = 0;
            var c = new DisposeController();
            c.OnDispose += () => count++;

            var build = new Build()
                .SetCulture(CultureInfo.InvariantCulture)
                .Recognize(c);
            build
                .Parse(new[] { "Dispose", "method" })
                .Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(0));
        }
    }
}
