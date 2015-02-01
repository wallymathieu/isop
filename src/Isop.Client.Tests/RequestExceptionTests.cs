using Isop.Client.Json;
using NUnit.Framework;
using System.Linq;

namespace Isop.Client.Tests
{
    [TestFixture]
    public class RequestExceptionTests
    {
        [Test]
        public void CanDeserializeMissingArgument()
        {
            var ex = new RequestException(System.Net.HttpStatusCode.BadRequest, "[{errorType:'MissingArgument', message:'msg', argument:'arg' }]");
            Assert.That(ex.ErrorObjects().First().GetType().Name, Is.EqualTo("MissingArgument"));
        }
        [Test]
        public void CanDeserializeTypeConversionFailed()
        {
            var ex = new RequestException(System.Net.HttpStatusCode.BadRequest, "[{errorType:'TypeConversionFailed', message:'msg', argument:'arg', value:'val', targetType:'int'}]");
            Assert.That(ex.ErrorObjects().First().GetType().Name, Is.EqualTo("TypeConversionFailed"));
        }

    }
}
