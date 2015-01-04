using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using System.Linq;
using With.Rubyfy;
using With;
using Nancy.Bootstrapper;
using Nancy.Helpers;
using Isop.Server;
namespace Isop.Wpf.Tests.Server
{
    [TestFixture]
    public class HtmlApiTests
    {
        private static Browser GetBrowser()
        {
            return GetBrowser<FakeIsopServer>();
        }
        private static Browser GetBrowser<TISopServer>() where TISopServer : class, IIsopServer
        {
            var bootstrapper = new TestBootstrapperWithIsopServer<TISopServer>();
            var browser = new Browser(bootstrapper);
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
            Assert.AreEqual(new[] { "Global" }, result.Body[".global_parameters input"]
                .Map(i => i.Attributes["name"]).ToA());
        }

        [Test]
        public void Should_return_available_controllers()
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
            Assert.AreEqual(new[] { "My" }, result.Body[".controllers a"]
                .Map(i => i.InnerText).ToA());
        }

        [Test]
        public void When_get_controller_url_Should_return_header_and_available_actions()
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
            Assert.AreEqual("My", result.Body["h1"].Single().InnerText.Trim('\n', '\r', ' '));
            Assert.AreEqual(new[] { "Action", "Fail", "ActionWithGlobalParameter", "ActionWithObjectArgument" },
                result.Body["a"].Map(i => i.InnerText.Trim('\n', '\r', ' ')).ToA());
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
            Assert.AreEqual("Action", result.Body["h1"].Single().InnerText.Trim('\n', '\r', ' '));
            var names = result.Body["form input"].Map(i => i.Attributes["name"]).ToA();
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
                : base(new Build { typeof(Isop.Tests.FakeControllers.SingleIntAction) })
            {
            }
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
        }
    }
}
