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
            public string Action(string name) { return null; }
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
        [Test]
        public void When_setting_value_on_global_parameter_will_set_value_on_method_parameter()
        {
            var treemodel = TreeModelWithCurrentMethodSelected();
            treemodel.GlobalParameters.First().Value = "new value";
            Assert.That(treemodel.CurrentMethod.Parameters.Single().Value, 
                Is.EqualTo("new value"));
        }
        [Test]
        public void When_setting_value_on_method_parameter_will_set_value_on_global_parameter()
        {
            var treemodel = TreeModelWithCurrentMethodSelected();
            treemodel.CurrentMethod.Parameters.Single().Value = "new value";
            Assert.That(treemodel.GlobalParameters.First().Value,
                Is.EqualTo("new value"));
        }
        private static MethodTreeModel TreeModelWithCurrentMethodSelected()
        {
            var treemodel = new Build()
                .Parameter(
                    new ArgumentParameter("name", new[] { "name" }))
                .Recognize(new MyController())
                .GetMethodTreeModel();
            treemodel.CurrentMethod = treemodel.Controllers.Single().Methods.Single();
            return treemodel;
        }
    }
}
