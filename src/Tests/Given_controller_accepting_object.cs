using System;
using System.IO;
using System.Linq;
using Isop;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Given_controller_accepting_object
    {
        class MyObjectController
        {
            public class Argument
            {
                public string param1 { get; set; }
                public string param2 { get; set; }
                public int param3 { get; set; }
                public decimal param4 { get; set; }
            }

            public MyObjectController()
            {
                OnAction = (a) => string.Empty;
            }
            public Func<Argument, string> OnAction { get; set; }
            public string Action(Argument a) { return OnAction(a); }
        }
        [Test]
        public void It_can_parse_class_and_method_with_object_and_execute()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new MyObjectController() { OnAction = (p1) => (count++).ToString() });

            var arguments = AppHostBuilder.Create(sc)
                .Recognize(typeof(MyObjectController))
                .BuildAppHost()
                .Parse(new[] { "MyObject", "Action", "--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4" });

            Assert.That(arguments.Unrecognized.Count, Is.EqualTo(0));
            arguments.Invoke(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }
    }
}