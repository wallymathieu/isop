using Isop;
using Lamar;
using NUnit.Framework;
using Tests.FakeControllers;

namespace Tests
{
    [TestFixture]
    public class Given_lamar_dependency_injection_framework
    {
        [Test]
        public void It_()
        {
            var container = new Container(_ =>
            {
                
            });
            var builder = Builder.Create(container.ServiceProvider).Recognize<MyController>()
                .BuildAppHost();
        }
    }
}