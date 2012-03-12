using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using TypeConverterFunc=System.Func<System.Type,string,System.Globalization.CultureInfo,object>;
namespace Isop.Tests
{
    class FullConfiguration:IDisposable
    {
        public bool DisposeCalled = false;
        
        public IEnumerable<Type> Recognizes()
        {
            return new[] { typeof(MyController) };
        }
        /// <summary>
        /// GLOBAL!!
        /// </summary>
        /// <value>
        /// The global.
        /// </value>
        //
        public string Global {
         get;
         set;
        }
		 [Required]
        public string GlobalRequired
        {
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
        public TypeConverterFunc GetTypeconverter()
        {
            return TypeConverter;
        }
        public static object TypeConverter(Type t, string s, CultureInfo c){ return null; }
    }
    [TestFixture]
    public class ConfigurationCanReadFullConfigurationTests
    {
        private Build parserBuilder;

        [SetUp]
        public void SetUp()
        {
            parserBuilder = new Build().Configuration(typeof(FullConfiguration));
        }
        [Test]
        public void RecognizeHelp()
        {
            Assert.That(parserBuilder.RecognizesHelp);
        }
        [Test]
        public void RecognizeCulture()
        {
            Assert.That(parserBuilder.Culture, Is.EqualTo(CultureInfo.GetCultureInfo("es-ES")));
        }
        [Test]
        public void RecognizeTypeConverter()
        {
            Assert.That(parserBuilder.TypeConverter, Is.EqualTo((TypeConverterFunc)FullConfiguration.TypeConverter));
        }
        [Test]
        public void RecognizeRecognizers()
        {
            Assert.That(parserBuilder.ControllerRecognizers.Select(cr => cr.Type).ToArray(),
                Is.EquivalentTo(new[] { typeof(MyController), typeof(HelpController) }));
        }
        [Test]
        public void RecognizeGlobalParameters()
        {
            var argumentWithOptionses = parserBuilder.GlobalParameters
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
        
        [Test] public void Can_read_configuration_from_example_project()
        {
            var path = Path.GetFullPath( Path.Combine("..","..","..",
                Path.Combine("Example.Cli","bin","Debug")));
            
            var parserBuilder = new Build().ConfigurationFrom(path);

            Assert.That(parserBuilder.RecognizesHelp);
            Assert.That(parserBuilder.ControllerRecognizers.Count(), 
                Is.AtLeast(2));
        }
        
        [Test,Ignore("Not really correct")] public void Can_use_file_location_to_get_directory()
        {
            var path = Assembly.GetExecutingAssembly().Location;
            Assert.That(Directory.GetParent(path).FullName,Is.EqualTo(Environment.CurrentDirectory));
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

        [Test, Ignore("need some way of testing this with albacore, resharper ... shadow copy")] public void Can_read_documentation_for_properties()
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
