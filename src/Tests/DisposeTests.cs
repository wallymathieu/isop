using System.Globalization;
using System.IO;
using Isop;
using NUnit.Framework;
using Tests.FakeControllers;

namespace Tests
{

    [TestFixture]
    public class DisposeTests
    {
        [Test]
        public void It_will_not_dispose_instances_set_to_be_recognized()
        {
            var count = 0;
            var c = new DisposeController();
            c.OnDispose += () => count++;

            var build = Builder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            }).Recognize(c).BuildAppHost();
            build
                .Parse(new[] { "Dispose", "method" })
                .Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(0));
        }
    }
}
