using System.IO;
using System.Linq;
using Isop;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tests.FakeControllers;
namespace Tests.ArgumentParsers
{
    [TestFixture]
    public class Given_controller_accepting_object
    {
        [Test,Ignore("Brown")]
        public void It_can_parse_class_and_method_with_object_and_execute()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyObjectController() { OnAction = (p1) => (count++).ToString() });

            var arguments = Builder.Create(sc)
                .Recognize(typeof(MyObjectController))
                .BuildAppHost()
                .Parse(new[] { "MyObject", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4" });

            Assert.That(arguments.UnRecognizedArguments.Count(), Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }
    }
}