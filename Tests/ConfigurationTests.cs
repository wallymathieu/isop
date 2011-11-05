using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Isop.Tests
{
    class FullConfiguration:IDisposable
    {
        private string _global;
        public IEnumerable<Type> Recognizes()
        {
            return new[] { typeof(MyController) };
        }
        public void Configure(string global)
        {
            this._global = global;
        }
        public object ObjectFactory(Type type)
        {
            return null;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
    [TestFixture]
    public class ConfigurationTests
    {
        [Test] public void Can_read_full_configuration()
        {
            var parserBuilder = ArgumentParser.BuildFromConfiguration(typeof(FullConfiguration));
            Assert.That(parserBuilder.GetControllerRecognizers().Select(cr => cr.Type), Is.EquivalentTo(new[] { typeof(MyController) }));
            Assert.That(parserBuilder.GetGlobalParameters().Select(p => p.Argument.Prototype.ToString()), Is.EquivalentTo(new[] { "global" }));
        }
    }
}
