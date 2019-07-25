using System.IO;
using Isop;
using Isop.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tests.FakeControllers;

namespace Tests
{
    [TestFixture]
    public class Given_controller_and_paratemers_with_all_arguments_fullfilled
    {
        private int count;
        private int countArg;
        private IParsed parsed;
        string[] args= { "My", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4", "--beta" };

        [SetUp]
        public void Setup()
        {
            count = 0;
            countArg = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyController() { OnAction = (p1, p2, p3, p4) => (count++).ToString() });

            parsed = AppHostBuilder.Create(sc)
                .Recognize(typeof(MyController))
                .Parameter("beta", arg => countArg++)
                .BuildAppHost()
                .Parse(args);
        }
        
        [Test]
        public void There_are_no_unrecognized_arguments()
        {
            Assert.That(parsed.Unrecognized.Count, Is.EqualTo(0));
        }
        [Test]
        public void It_will_execute_action_once()
        {
            parsed.Invoke(new StringWriter());
            Assert.That(count, Is.AtLeast(1));
        }
        [Test]
        public void It_will_only_execute_parameter_at_least_once()
        {
            parsed.Invoke(new StringWriter());
            Assert.That(countArg, Is.AtLeast(1));
        }
        [Test]
        public void It_will_only_execute_parameter_exactly_once()
        {
            parsed.Invoke(new StringWriter());
            Assert.That(countArg, Is.EqualTo(1));
        }
    }
}