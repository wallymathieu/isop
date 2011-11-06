using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Isop.Tests
{
    class FullConfiguration:IDisposable
    {
        public bool DisposeCalled = false;
        
        public IEnumerable<Type> Recognizes()
        {
            return new[] { typeof(MyController) };
        }
        public string Global {
         get;
         set;
        }
        public object ObjectFactory(Type type)
        {
            return null;
        }
        public CultureInfo Culture
        {
            get{ return CultureInfo.GetCultureInfo("es-ES"); }
        }
        public bool RecognizeHelp{get{return true;}}
        public void Dispose()
        {
            DisposeCalled = true;
        }
        // TypeConverterFunc typeconverter=

    }
    [TestFixture]
    public class ConfigurationTests
    {
        [Test] public void Can_read_full_configuration()
        {
            var parserBuilder = new Build().Configuration(typeof(FullConfiguration));
            Assert.That(parserBuilder.RecognizesHelp());
            Assert.That(parserBuilder.GetCulture(),Is.EqualTo(CultureInfo.GetCultureInfo("es-ES")));
            Assert.That(parserBuilder.GetControllerRecognizers().Select(cr => cr.Type).ToArray(), 
                Is.EquivalentTo(new[] { typeof(MyController), typeof(HelpController) }));
            Assert.That(parserBuilder.GetGlobalParameters().Select(p => p.Argument.Prototype.ToString()).ToArray(),
                Is.EquivalentTo(new[] { "Global" }));
        }
        
        [Test] public void Can_dispose_of_configuration_after_usage()
        {
            var conf = new FullConfiguration();
            var parserBuilder = new Build().Configuration(conf);
            parserBuilder.Dispose();
            Assert.That(conf.DisposeCalled);
        }
        
        [Test] public void Can_read_configuration_from_example_project()
        {
            var path = Path.Combine("..","..","..",
                Path.Combine("Example","bin","Debug"));
            
            var parserBuilder = new Build().ConfigurationFrom(path);

            Assert.That(parserBuilder.RecognizesHelp());
            Assert.That(parserBuilder.GetControllerRecognizers().Count(), 
                Is.AtLeast(1));
        }
        
        [Test] public void Can_use_file_location_to_get_directory()
        {
            var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Assert.That(Directory.GetParent(path).FullName,Is.EqualTo(Environment.CurrentDirectory));
        }
        
        [Test] public void Can_invoke_configure_method_on_configuration()
        {
            var conf = new FullConfiguration();
            var parserBuilder = new Build().Configuration(conf);
            var parsed = parserBuilder.Parse(new []{"--global","globalvalue","My","Action","--value","1"});
            var cout = new StringWriter();
            parsed.Invoke(cout);
            Assert.That(conf.Global,Is.EqualTo("globalvalue"));
        }
    }
}
