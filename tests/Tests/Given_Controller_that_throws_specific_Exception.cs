using System.Globalization;
using System.IO;
using Isop;
using Isop.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Given_Controller_that_throws_specific_Exception
    {
        private IParsed? _parsed;

        public class SpecificException : System.Exception
        {
        }

        private class ObjectController
        {
            public object Action()
            {
                throw new SpecificException();
            }
        }
        [SetUp]
        public void SetUp()
        {
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new ObjectController());

            _parsed = AppHostBuilder.Create(sc, new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            }).Recognize<ObjectController>()
                .BuildAppHost()
                .Parse(["Object", "Action"]);
        }
        [Test]
        public void It_recognizes_all_parameters() =>
            Assert.That(_parsed!.Unrecognized.Count, Is.EqualTo(0));

        [Test]
        public void It_throws_correct_exception_sync() =>
            Assert.ThrowsAsync<SpecificException>(async () => await _parsed!.InvokeAsync(new StringWriter()));

        [Test]
        public void It_throws_correct_exception_async() =>
            Assert.ThrowsAsync<SpecificException>(async () => await _parsed!.InvokeAsync(new StringWriter()));
    }
}
