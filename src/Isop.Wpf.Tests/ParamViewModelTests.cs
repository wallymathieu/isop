using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using Isop.Gui.ViewModels;

namespace Isop.Wpf.Tests
{
    [TestFixture]
    public class ParamViewModelTests
    {
        private Isop.Client.Models.Param GetParam()
        {
            return new Isop.Client.Models.Param() { Type = typeof(string).FullName, Name = "name" };
        }

        [Test]
        public void Changing_the_value_of_param_will_trigger_callback() 
        {
            var p = new ParamViewModel(GetParam());
            bool triggered = false;
            p.PropertyChanged += (s, arg) => { triggered = true; };
            p.Value = "val";
            Assert.That(triggered);
        }

        [Test]
        public void Changing_the_value_of_param_without_callback()
        {
            var p = new ParamViewModel(GetParam());
            p.Value = "val";
        }

    }
}
