using Isop.Server;
using NUnit.Framework;
using System.Collections.Generic;
using With;
namespace Isop.Tests.Server
{
    [TestFixture]
    public class IsopServerFromBuildTests
    {
        private IsopServerFromBuild _data;
        public class MyController
        {
            public string Action(string value)
            {
                return value;
            }
        }

        [SetUp]
        public void SetUp()
        {
            _data = new IsopServerFromBuild( ()=> new Build().Recognize<MyController>());
        }

        [Test]
        public void InvokeMethod()
        {
            var method = _data.GetControllerMethod("My", "Action");
            var value = "value ' 3 ' \"_12 \"sdf";
            var result = _data.InvokeMethod(method, new Dictionary<string, object> { { "value", value } }).Join("\n");
            Assert.That(result, Is.EqualTo(value));
        }
    }
}
