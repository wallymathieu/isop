using System.Collections.Generic;
using System.Linq;
using Isop.Tests.FakeConfigurations;
using Isop.Tests.FakeControllers;
using NUnit.Framework;
using System.Globalization;
using TypeConverterFunc = System.Func<System.Type, string, System.Globalization.CultureInfo, object>;
using Isop.Controllers;

namespace Isop.Tests
{
    [TestFixture]
    public class ConfigurationCanReadFullConfigurationTests
    {
        private Build _parserBuilder;

        [SetUp]
        public void SetUp()
        {
            _parserBuilder = new Build().Configuration(typeof(FullConfiguration));
        }
        [Test]
        public void RecognizeHelp()
        {
            Assert.That(_parserBuilder.RecognizesHelp);
        }
        [Test]
        public void RecognizeCulture()
        {
            Assert.That(_parserBuilder.CultureInfo, Is.EqualTo(CultureInfo.GetCultureInfo("es-ES")));
        }
        [Test]
        public void RecognizeTypeConverter()
        {
            Assert.That(_parserBuilder.TypeConverter, Is.EqualTo((TypeConverterFunc)FullConfiguration.TypeConverter));
        }
        [Test]
        public void RecognizeRecognizers()
        {
            Assert.That(_parserBuilder.ControllerRecognizers.Select(cr => cr.Key).ToArray(),
                Is.EquivalentTo(new[] { typeof(MyController), typeof(HelpController) }));
        }
        [Test]
        public void RecognizeGlobalParameters()
        {
            var argumentWithOptionses = _parserBuilder.GlobalParameters
                .ToArray();

            var requiredPairs = argumentWithOptionses
                .Select(p => new KeyValuePair<string, bool>(p.Argument.Prototype.ToString(), p.Required))
                .ToArray();
            Assert.That(requiredPairs,
                Is.EquivalentTo(new[] { 
                    new KeyValuePair<string, bool>("Global", false),
                    new KeyValuePair<string, bool>("GlobalRequired",true) 
                }));
        }
    }
}
