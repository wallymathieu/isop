using System.Linq;
using Isop.Parse;
using NUnit.Framework;
using Isop.Gui.ViewModels;
using Isop.Gui;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Isop.Wpf.ServerIntegration
{
    [TestFixture]
    public class RootViewModelTests
    {
        class JsonHttpClientThatOnlyReturns : IJSonHttpClient
        {
            private string data;

            public JsonHttpClientThatOnlyReturns(string data)
            {
                this.data = data;
            }
            public Task<JsonResponse> Request(Request request)
            {
                return Task.FromResult(new JsonResponse(System.Net.HttpStatusCode.OK, data));
            }
        }

        private RootViewModel GetRootModelFromBuild(Build b)
        {
            var server = new Isop.Server.IsopServerFromBuild( ()=> b );
            var data = JsonConvert.SerializeObject(server.GetModel());
            return new IsopClient(new JsonHttpClientThatOnlyReturns(data), "http://localhost:666").GetMethodTreeModel().Result;
        }

        private class MyController
        {
            public string Action(string name) { return null; }
            public string AnotherAction(string nameInAnother) { return null; }
        }

        [Test]
        public void Can_get_vm_from_configuration()
        {
            var treemodel = GetRootModelFromBuild(new Build()
                .Parameter(
                    new ArgumentParameter("name", new[] { "name" })));
            Assert.That(treemodel.GlobalParameters.Count(), Is.EqualTo(1));

            Assert.That(treemodel.GlobalParameters.Select(p => p.Name),
                Is.EquivalentTo(new[] { "name" }));
        }

        [Test]
        public void Can_get_vm_from_configuration_with_controllers()
        {
            var treemodel = GetRootModelFromBuild(new Build()
                .Recognize(new MyController()));

            Assert.That(treemodel.Controllers.Select(p => p.Name),
                Is.EquivalentTo(new[] { "My" }));
            Assert.That(treemodel.Controllers.Single().Methods.Select(p => p.Name),
                Is.EquivalentTo(new[] { "Action", "AnotherAction" }));
            Assert.That(treemodel.Controllers.Single().Methods.SelectMany(p => p.Parameters.Select(p2 => p2.Name)),
                Is.EquivalentTo(new[] { "name", "nameInAnother" }));
        }
    }
}
