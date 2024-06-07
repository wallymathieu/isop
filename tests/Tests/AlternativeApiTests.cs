using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Isop;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tests.FakeControllers;

namespace Tests
{
    [TestFixture]
    public class AlternativeApiTests
    {
        [Test]
        public async Task It_can_parse_and_invoke()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci=>new MyController() { OnAction = (p1, p2, p3, p4) => (count++).ToString() });
            var arguments = AppHostBuilder.Create(sc,new Configuration {
                CultureInfo= CultureInfo.InvariantCulture
            })
            .Recognize<MyController>()
            .BuildAppHost()
            .Controller("My")
            .Action("Action")
            .Parameters(new Dictionary<string, string?> { { "param1", "value1" }, { "param2", "value2" }, { "param3", "3" }, { "param4", "3.4" } });

            Assert.That(arguments.Unrecognized.Select(u=>u.Value), Is.Empty);
            await arguments.InvokeAsync(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }
        [Test]
        public void It_can_get_help()
        {
            var help = AppHostBuilder.Create(new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            })
            .Recognize<MyController>()
            .BuildAppHost()
            .Controller("My")
            .Action("Action")
            .Help();

            Assert.IsNotEmpty(help);
        }
        [Test]
        public void It_can_list_controllers()
        {
            var controllers = AppHostBuilder.Create(new Configuration
                {
                    CultureInfo = CultureInfo.InvariantCulture
                })
                .Recognize<MyController>()
                .BuildAppHost()
                .Controllers.Select(c=>c.Name).ToArray();

            CollectionAssert.AreEqual(new []{"Help","My"}, controllers);
        }
        [Test]
        public void It_can_list_actions()
        {
            var actions = AppHostBuilder.Create(new Configuration
                {
                    CultureInfo = CultureInfo.InvariantCulture
                })
                .Recognize<MyController>()
                .BuildAppHost()
                .Controllers.Single(c=>c.Name=="My").Actions.Select(a=>a.Name).ToArray();

            CollectionAssert.AreEqual(new []{"Action"}, actions);
        }
        [Test]
        public void It_can_list_action_parameters()
        {
            var actions = AppHostBuilder.Create(new Configuration
                {
                    CultureInfo = CultureInfo.InvariantCulture
                })
                .Recognize<MyController>()
                .BuildAppHost()
                .Controllers.Single(c=>c.Name=="My").Actions.Single(a=>a.Name=="Action").Arguments.Select(a=>a.Name).ToArray();

            CollectionAssert.AreEqual(new []{"param1","param2","param3","param4"}, actions);
        }
    }
}
