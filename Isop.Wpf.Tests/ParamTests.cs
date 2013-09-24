using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Isop.Parse;
using NUnit.Framework;
using Isop.WpfControls.ViewModels;
using System.ComponentModel;

namespace Isop.Wpf.Tests
{
    [TestFixture]
    public class ParamTests
    {
        [Test]
        public void Changing_the_value_of_param_will_trigger_callback() 
        {
            var p = new Param(typeof(string), "name", new ArgumentWithOptions(new ArgumentParameter("name", new []{"name"})));
            bool triggered = false;
            p.PropertyChanged += (s, arg) => { triggered = true; };
            p.Value = "val";
            Assert.That(triggered);
        }
        [Test]
        public void Changing_the_value_of_param_without_callback()
        {
            var p = new Param(typeof(string), "name", new ArgumentWithOptions(new ArgumentParameter("name", new[] { "name" })));
            p.Value = "val";
        }

        [Test]
        public void Get_parsed_arguments() 
        {
            var p = new Param(typeof(string), "name", new ArgumentWithOptions(new ArgumentParameter("name", new[] { "name" })));
            var c = new[]{ p }.GetParsedArguments();
            Assert.That(c.RecognizedArguments.Count(), Is.EqualTo(0));
            Assert.That(c.UnRecognizedArguments.Count(), Is.EqualTo(0));
            Assert.That(c.ArgumentWithOptions.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Get_parsed_arguments_when_value_is_set()
        {
            var p = new Param(typeof(string), "name", new ArgumentWithOptions(new ArgumentParameter("name", new[] { "name" })));
            p.Value = "value";
            var c = new[] { p }.GetParsedArguments();
            Assert.That(c.RecognizedArguments.Count(), Is.EqualTo(1));
            Assert.That(c.UnRecognizedArguments.Count(), Is.EqualTo(0));
            Assert.That(c.ArgumentWithOptions.Count(), Is.EqualTo(1));
        }
    }
}
