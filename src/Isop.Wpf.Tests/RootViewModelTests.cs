using System.Linq;
using NUnit.Framework;
using Isop.Gui.ViewModels;
using With;
using Isop.Gui;
using Newtonsoft.Json;
using Isop.Gui.Models;
using System.Threading.Tasks;
namespace Isop.Wpf.Tests
{
    [TestFixture]
    public class RootViewModelTests
    {
        private RootViewModel GetMethodTreeModel(Root b)
        {
            var data = JsonConvert.SerializeObject(b);
            return new IsopClient(new JsonHttpClientThatOnlyReturns(data), "http://localhost:666").GetMethodTreeModel().Result;
        }

        private RootViewModel TreeModelWithCurrentMethodSelected()
        {
            var treemodel = GetMethodTreeModel(TreeModel());

            treemodel.CurrentMethod = treemodel.Controllers.Single()
                .Methods.Single(m => m.Name == "Action");
            return treemodel;
        }

        private static Root TreeModel()
        {
            return new Root
            {
                GlobalParameters = new[] { new Param { Name = "name", Type = typeof(string).FullName } },
                Controllers = new[]{
                    new Controller
                    {
                        Name="My", 
                        Methods=new []{
                            new Method{Name="Action", Parameters=new []{new Param{Name="name", Type=typeof(string).FullName}}},
                            new Method{Name="AnotherAction", Parameters=new []{new Param{Name="name", Type=typeof(string).FullName}}}
                        }
                    }
                }
            };
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
        [Test]
        public void When_selecting_another_action_will_deregister_event_handlers()
        {
            var treemodel = TreeModelWithCurrentMethodSelected();
            var param1 = treemodel.CurrentMethod.Parameters.Single();
            param1.Value = "new value";
            treemodel.CurrentMethod = treemodel.Controllers.Single().Methods.Single(m => m.Name == "AnotherAction");
            treemodel.CurrentMethod.Parameters.Single().Value = "another value";
            Assert.That(treemodel.GlobalParameters.First().Value,
                Is.EqualTo("another value"));
            Assert.That(param1.Value, Is.EqualTo("new value"));
        }
        /// <summary>
        /// The parameters might be regenerated when using Select/Where
        /// </summary>
        [Test]
        public void Parameters_does_not_change()
        {
            var treemodel = TreeModelWithCurrentMethodSelected();
            var param1 = treemodel.CurrentMethod.Parameters.Single();
            treemodel.CurrentMethod = treemodel.Controllers.Single().Methods.Single(m => m.Name == "AnotherAction");
            Assert.That(treemodel.Controllers.Single()
                .Methods.Single(m => m.Name == "Action").Parameters.Single(),
                Is.EqualTo(param1));
        }
    }
}
