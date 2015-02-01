using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Isop.Client.Json;
using Isop.Client;
using Isop.Client.Models;

namespace Isop.Client.Tests
{
    [TestFixture]
    public class SerializationTests
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

        private Root GetRootModelFromBuild(Build b)
        {
            var server = new Isop.Server.IsopServerFromBuild( ()=> b );
            var data = JsonConvert.SerializeObject(server.GetModel());
            var client = new IsopClient(new JsonHttpClientThatOnlyReturns(data), "http://localhost:666");
            return client.GetModel().Result;
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
                .Parameter("name"));
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
