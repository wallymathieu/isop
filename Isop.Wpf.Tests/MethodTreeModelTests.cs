using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Isop.WpfControls.ViewModels;

namespace Isop.Wpf.Tests
{
    [TestFixture]
    public class MethodTreeModelTests
    {
        [Test]
        public void Can_generate_tree_model_from_configuration() 
        {
            var treemodel = new Build()
                .Parameter(
                    new ArgumentParameter("name", new[] { "name" }))
                .GetMethodTreeModel();
            Assert.That(treemodel.Controllers.Count(), Is.EqualTo(0));
            Assert.That(treemodel.GlobalParameters.Count(), Is.EqualTo(1));
        }
        private class MyController
        {
            public string Action() { return null; }
        }
        [Test]
        public void Can_generate_tree_model_from_configuration_with_controllers()
        {
            var treemodel = new Build()
                .Recognize(new MyController())
                .GetMethodTreeModel();
            Assert.That(treemodel.Controllers.Count(), Is.EqualTo(1));
            Assert.That(treemodel.GlobalParameters.Count(), Is.EqualTo(0));
        }
    }
}
