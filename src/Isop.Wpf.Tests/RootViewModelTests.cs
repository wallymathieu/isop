using System.Linq;
using NUnit.Framework;
using Isop.Gui.ViewModels;
using Isop.Gui;
using Isop.Client.Transfer;
using FakeItEasy;
using Isop.Client;
using Isop.Gui.Adapters;
namespace Isop.Wpf.Tests
{
    [TestFixture]
    public class RootViewModelTests
    {
        private IIsopClient _isopClient;

        private RootViewModel RootVmWithMethodSelected()
        {
            var mt = RootModelFromSource();
            var treemodel = new RootViewModel(new JsonClient(_isopClient), mt);

            treemodel.CurrentMethod = treemodel.Controllers.Single()
                .Methods.Single(m => m.Name == "Action");
            return treemodel;
        }

        private static Root RootModelFromSource()
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
        [SetUp]
        public void SetUp()
        {
            _isopClient = A.Fake<IIsopClient>();
        }

        [Test]
        public void When_setting_value_on_global_parameter_will_set_value_on_method_parameter()
        {
            var treemodel = RootVmWithMethodSelected();
            treemodel.GlobalParameters.First().Value = "new value";
            Assert.That(treemodel.CurrentMethod.Parameters.Single().Value,
                Is.EqualTo("new value"));
        }
        [Test]
        public void When_setting_value_on_method_parameter_will_set_value_on_global_parameter()
        {
            var treemodel = RootVmWithMethodSelected();
            treemodel.CurrentMethod.Parameters.Single().Value = "new value";
            Assert.That(treemodel.GlobalParameters.First().Value,
                Is.EqualTo("new value"));
        }
        [Test]
        public void When_selecting_another_action_will_deregister_event_handlers()
        {
            var treemodel = RootVmWithMethodSelected();
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
            var treemodel = RootVmWithMethodSelected();
            var param1 = treemodel.CurrentMethod.Parameters.Single();
            treemodel.CurrentMethod = treemodel.Controllers.Single().Methods.Single(m => m.Name == "AnotherAction");
            Assert.That(treemodel.Controllers.Single()
                .Methods.Single(m => m.Name == "Action").Parameters.Single(),
                Is.EqualTo(param1));
        }
    }
}
