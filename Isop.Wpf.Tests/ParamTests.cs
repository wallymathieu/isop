using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
