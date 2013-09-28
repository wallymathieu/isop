using System.Collections.Generic;
using System.Linq;
using Isop.Controller;
using NUnit.Framework;
using System.Globalization;
using System.IO;
using TypeConverterFunc=System.Func<System.Type,string,System.Globalization.CultureInfo,object>;
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
            Assert.That(_parserBuilder.Culture, Is.EqualTo(CultureInfo.GetCultureInfo("es-ES")));
        }
        [Test]
        public void RecognizeTypeConverter()
        {
            Assert.That(_parserBuilder.TypeConverter, Is.EqualTo((TypeConverterFunc)FullConfiguration.TypeConverter));
        }
        [Test]
        public void RecognizeRecognizers()
        {
            Assert.That(_parserBuilder.ControllerRecognizers.Select(cr => cr.Type).ToArray(),
                Is.EquivalentTo(new[] { typeof(MyController), typeof(HelpController) }));
        }
        [Test]
        public void RecognizeGlobalParameters()
        {
            var argumentWithOptionses = _parserBuilder.GlobalParameters
                .ToArray();

            var requiredPairs = argumentWithOptionses
                .Select(p => new KeyValuePair<string,bool>( p.Argument.Prototype.ToString(),p.Required))
                .ToArray();
            Assert.That(requiredPairs,
                Is.EquivalentTo(new[] { 
                    new KeyValuePair<string, bool>("Global", false),
                    new KeyValuePair<string, bool>("GlobalRequired",true) 
                }));
        }
    }
    [TestFixture]
    public class ConfigurationTests
    {
        [Test] public void Can_dispose_of_configuration_after_usage()
        {
            var conf = new FullConfiguration();
            var parserBuilder = new Build().Configuration(conf);
            parserBuilder.Dispose();
            Assert.That(conf.DisposeCalled);
        }
        
        [Test
#if !APPHARBOR
        ,Ignore("APPHARBOR")
#endif
        ] public void Can_read_configuration_from_example_project()
        {
            var path = Path.GetFullPath( Path.Combine("..","..","..",
                Path.Combine("Example.Cli","bin","Debug")));
            
            var parserBuilder = new Build().ConfigurationFrom(path);

            Assert.That(parserBuilder.RecognizesHelp);
            Assert.That(parserBuilder.ControllerRecognizers.Count(), 
                Is.AtLeast(2));
        }
        
        [Test] public void Can_invoke_configure_method_on_configuration()
        {
            var conf = new FullConfiguration();
            var parserBuilder = new Build().Configuration(conf);
            var parsed = parserBuilder.Parse(new []{"--global","globalvalue","--globalrequired","2","My","Action","--value","1"});
            var cout = new StringWriter();
            parsed.Invoke(cout);
            Assert.That(conf.Global,Is.EqualTo("globalvalue"));
        }

        [Test
#if !APPHARBOR
        ,Ignore("APPHARBOR")
#endif
        ] public void Can_read_documentation_for_properties()
        {
            var conf = new FullConfiguration();
            var parserBuilder = new Build().Configuration(conf);
            var globalDesc = parserBuilder.GlobalParameters
                .First(gp=>gp.Argument.Prototype.Equals("Global")).Description;
            Assert.That(globalDesc,Is.EqualTo("GLOBAL!!"));
        }
        [Test] public void Can_use_autoconfiguration()
        {
            var recognizes = new IsopAutoConfiguration(this.GetType().Assembly).Recognizes().ToArray();
            Assert.That(recognizes,Is.EquivalentTo(new []{typeof(MyController)}));
        }
    }
}
