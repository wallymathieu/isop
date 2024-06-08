using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Isop;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests.Routing
{
    public class Given_controller_with_index_action
    {  
        private class WithIndexController
        {
            public WithIndexController()
            {
                OnIndex = (p1, p2, p3, p4) => string.Empty;
            }
            public Func<string, string, int, decimal, string> OnIndex { get; set; }
            public string Index(string param1, string param2, int param3, decimal param4) { return OnIndex(param1, param2, param3, param4); }
        }
        
        [Test]
        public async Task It_can_parse_class_and_default_method_and_execute()
        {
            var count = 0;
            var sc = new ServiceCollection();
            sc.AddSingleton(ci => new WithIndexController { OnIndex = (p1, p2, p3, p4) => (count++).ToString() });

            var arguments = AppHostBuilder.Create(sc, new AppHostConfiguration { CultureInfo = CultureInfo.InvariantCulture })
                .Recognize(typeof(WithIndexController))
                .BuildAppHost()
                .Parse(new[] { "WithIndex", /*"Index", */"--param2", "value2", "--param3", "3", "--param1", "value1", "--param4", "3.4" });

            Assert.That(arguments.Unrecognized.Select(u=>u.Value), Is.Empty);
            await arguments.InvokeAsync(new StringWriter());
            Assert.That(count, Is.EqualTo(1));
        }
    }
}