using Isop.Server;
using NUnit.Framework;
using System.Collections.Generic;
using With;
using System;
namespace Isop.Tests.Server
{
    [TestFixture]
    public class IsopServerFromBuildTests
    {
        private IsopServerFromBuild _data;
        private MyController _controller;
        public class MyController
        {
            public string Action(string value)
            {
                return value;
            }
            public Object Value;
            public Object ReturnObject()
            {
                return Value;
            }
        }

        [SetUp]
        public void SetUp()
        {
            _controller = new MyController();
            _data = new IsopServerFromBuild( ()=> new Build().Recognize(_controller));
        }

        [Test]
        public void PassingValueShouldNotDistortParameter()
        {
            var method = _data.GetControllerMethod("My", "Action");
            var value = "value ' 3 ' \"_12 \"sdf";
            var result = _data.InvokeMethod(method, new Dictionary<string, object> { { "value", value } }).Join("\n");
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public void FormatStringAsString()
        {
            var method = _data.GetControllerMethod("My", "ReturnObject");
            var value = "value ' 3 ' \"_12 \"sdf";
            _controller.Value = value;
            var result = _data.InvokeMethod(method, Empty()).Join("\n");
            Assert.That(result, Is.EqualTo(value));
        }

        [Test]
        public void FormatObjectAsJson()
        {
            var method = _data.GetControllerMethod("My", "ReturnObject");
            var value = "{\"v\":1}";
            _controller.Value = new {v=1};
            var result = _data.InvokeMethod(method, Empty()).Join("\n");
            Assert.That(result, Is.EqualTo(value));
        }

        private Dictionary<string, object> Empty(){
            return new Dictionary<string, object>();
        }
    }
}
