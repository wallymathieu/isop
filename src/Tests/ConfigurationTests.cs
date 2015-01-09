using System.Linq;
using Isop.Tests.FakeConfigurations;
using Isop.Tests.FakeControllers;
using NUnit.Framework;
using System.IO;

namespace Isop.Tests
{
    using Configurations;
    using Infrastructure;

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
        
        [Test] public void Can_read_configuration_from_example_project()
        {
            var path = Path.GetFullPath( Path.Combine("..","..","..",
                Path.Combine("Example","bin","Debug")));
            
            var parserBuilder = new Build().ConfigurationFrom(path);

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

        [Test] public void Can_read_documentation_for_properties()
        {
            var conf = new FullConfiguration();
            var parserBuilder = new Build().Configuration(conf);
            var globalDesc = parserBuilder.GlobalParameters
                .First(gp=>gp.Argument.Prototype.Equals("Global")).Description;
            Assert.That(globalDesc,Is.EqualTo("GLOBAL!!"));
        }
        [Test] public void Can_use_autoconfiguration()
        {
            var recognizes = new AssemblyScanner(this.GetType().Assembly).LooksLikeControllers().ToArray();
            Assert.That(recognizes,Is.EquivalentTo(new []{typeof(MyController)}));
        }
    }
}
