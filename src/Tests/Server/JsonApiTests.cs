using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using System.Linq;
using With.Rubyfy;
using With;
using Nancy.Bootstrapper;
using Nancy.Helpers;
using Isop.Server;
using System.Collections.Generic;

namespace Isop.Tests.Server
{
    [TestFixture]
    public class JsonApiTests
    {
        private static Browser GetBrowser()
        {
            return GetBrowser<FakeIsopServer>();
        }
        private static Browser GetBrowser<TISopServer>() where TISopServer : class,IIsopServer
        {
            var bootstrapper = new TestBootstrapperWithIsopServer<TISopServer>();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/json"));
            return browser;
        }
        [Test]
        public void Should_return_global_parameters()
        {
            // Given
            var browser = GetBrowser();

            // When
            var result = browser.Get("/", with =>
            {
                with.HttpRequest();
            });

            // Then
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(new[] { "Global" }, result.Body.DeserializeJson<Isop.Server.Models.MethodTreeModel>().GlobalParameters
                .Map(i => i.Name).ToA());
        }

        [Test]
        public void Should_return_controllers()
        {
            // Given
            var browser = GetBrowser();

            // When
            var result = browser.Get("/", with =>
            {
                with.HttpRequest();
            });

            // Then
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(new[] { "My" }, result.Body.DeserializeJson<Isop.Server.Models.MethodTreeModel>().Controllers
                .Map(i => i.Name).ToA());
        }

        [Test]
        public void When_get_controller_url_Should_return_available_actions()
        {
            // Given
            var browser = GetBrowser();

            // When
            var result = browser.Get("/My/", with =>
            {
                with.HttpRequest();
            });

            // Then
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var response = result.Body.DeserializeJson<Isop.Server.Models.Controller>();
            Assert.AreEqual("My", response.Name);
            Assert.AreEqual(new[] { "Action", "Fail", "ActionWithGlobalParameter", "ActionWithObjectArgument" },
                response.Methods.Map(i => i.Name).ToA());
        }

        [Test]
        public void Form_for_action_Should_contain_parameters()
        {
            // Given
            var browser = GetBrowser();

            // When
            var result = browser.Get("/My/Action/", with =>
            {
                with.HttpRequest();
            });

            // Then
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var response = result.Body.DeserializeJson<Isop.Server.Models.Method>();

            Assert.AreEqual("Action", response.Name);
            var names = response.Parameters.Map(i => i.Name).ToA();
            Assert.AreEqual(new[] { "value" }, names);
        }


        [Test]
        public void Post_form_action()
        {
            // Given
            var browser = GetBrowser();

            var value = "value ' 3 ' \"_12 \"sdf";

            // When
            var result = browser.Post("/My/Action/", with =>
            {
                with.HttpRequest();
                with.FormValue("value", value);
            });

            // Then
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.That(result.Body["p"].Map(p => p.InnerText).Join("\n"), Is.StringContaining(HttpUtility.HtmlEncode("value=" + value)));
        }

        class FakeIsopServerWithSingleIntAction : IsopServerFromBuild
        {
            public FakeIsopServerWithSingleIntAction()
                : base( ()=> new Build { typeof(Isop.Tests.FakeControllers.SingleIntAction) })
            {
            }
        }
        public void Post_form_action2()
        {
            // Given
            var browser = GetBrowser<FakeIsopServerWithSingleIntAction>();

            // When
            var result = browser.Post("/SingleIntAction/Action/", with =>
            {
                with.HttpRequest();
                with.FormValue("param", "1");
            });

            // Then
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [Test]
        public void Post_form_action_with()
        {
            // Given
            var browser = GetBrowser<FakeIsopServerWithSingleIntAction>();

            // When
            var result = browser.Post("/SingleIntAction/Action/", with =>
            {
                with.HttpRequest();
            });

            // Then
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual("MissingArgument", result.Headers["ErrorType"]);
            var response = result.Body.DeserializeJson<Isop.Server.Models.MissingArgument>();

            Assert.That(response.Arguments, Is.EquivalentTo(new[] { "param" }));
        }

        [Test]
        public void Post_form_action_with_wrong_value()
        {
            // Given
            var browser = GetBrowser<FakeIsopServerWithSingleIntAction>();

            // When
            var result = browser.Post("/SingleIntAction/Action/", with =>
            {
                with.HttpRequest();
                with.FormValue("param", "asdf");
            });

            // Then
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual("TypeConversionFailed", result.Headers["ErrorType"]);
            var response = result.Body.DeserializeJson<Isop.Server.Models.TypeConversionFailed>();

            Assert.That(response.Argument, Is.EqualTo("param"), "Arg");
            Assert.That(response.TargetType, Is.EqualTo("System.Int32"), "TargetType");
            Assert.That(response.Value, Is.EqualTo("asdf"), "Value");
        }
    }
}
