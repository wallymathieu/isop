using System.Linq;
using NUnit.Framework;
using With;
using Isop.Gui;
using Newtonsoft.Json;
using System.IO;
using System;
using Isop.Gui.Models;
using System.Threading.Tasks;
using System.Net;
using Isop.Gui.Http;
using Isop.Gui.ViewModels;

namespace Isop.Wpf.Tests
{
    [TestFixture]
    public class InvokeMethodViewModelTests
    {

        class HttpClientThatReturnsNoiseOnPost : JsonHttpClientThatOnlyReturns
        {
            public HttpClientThatReturnsNoiseOnPost(string data)
                : base(data)
            {
            }
            public override Task<JsonResponse> Request(Request request)
            {
                if (request.Post)
                {
                    var stream = new MemoryStream();
                    var writer = new StreamWriter(stream);
                    writer.Write("Just some noise");
                    stream.Position = 0;
                    return Task.FromResult(new JsonResponse(HttpStatusCode.OK, (Stream)stream));
                }
                return base.Request(request);
            }
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

        private RootViewModel GetMethodTreeModelFromBuild(Root b)
        {
            var data = JsonConvert.SerializeObject(b);
            return new IsopClient(new HttpClientThatReturnsNoiseOnPost(data), "http://localhost:666").GetMethodTreeModel().Result;
        }

        private RootViewModel TreeModelWithCurrentMethodSelected()
        {
            var treemodel = GetMethodTreeModelFromBuild(TreeModel());

            treemodel.CurrentMethod = treemodel.Controllers.Single()
                .Methods.Single(m => m.Name == "Action");
            return treemodel;
        }

        [Test]
        public void Can_run_selected_method()
        {
            var treemodel = TreeModelWithCurrentMethodSelected();
            var param1 = treemodel.CurrentMethod.Parameters.Single();
            param1.Value = "new value";
            var result = treemodel.CurrentMethod.Invoke().Result;
        }
        [Test]
        public void Can_run_action_with_object_argument()
        {
            var treemodel = GetMethodTreeModelFromBuild(TreeModel());
            treemodel.CurrentMethod = treemodel.Controllers.Single()
                .Methods.Single(m => m.Name == "Action");
            treemodel.CurrentMethod.Parameters.Single().Value = "123";
            var result = treemodel.CurrentMethod.Invoke().Result;
        }

        //class HttpClientThatErrors : IHttpClient
        //{
        //    public Task<HttpResponse> Request(Request request)
        //    {
        //        return Task.FromResult(new HttpResponse(new RequestError(HttpStatusCode.BadRequest, @"{}")));
        //    }
        //}

        [Test]
        public void Will_get_error_as_a_value()
        {
            Assert.Inconclusive(" test not implemented ");
            //var treemodel = GetMethodTreeModelFromBuildAndWithResponse(TreeModel(), new HttpClientThatErrors());
            //treemodel.CurrentMethod = treemodel.Controllers.Single()
            //    .Methods.Single(m => m.Name == "Action");
            //Assert.That( treemodel.CurrentMethod.Invoke().Result,
            //    Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }
}
