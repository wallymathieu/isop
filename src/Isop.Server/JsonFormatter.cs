using System;
using System.Collections.Generic;
using System.Linq;
using Isop.Infrastructure;
using Isop.Server.Models;
using System.Reflection;
using Newtonsoft.Json;

namespace Isop.Server
{
	public class JsonFormatter
	{
        private readonly CustomJsonSerializer _serializer;
        public JsonFormatter()
        {
            _serializer = new CustomJsonSerializer();
            _serializer.Formatting = Formatting.None;
        }

        public IEnumerable<string> Format(object retval)
        {
            yield return _serializer.Serialize(retval);
        }
	}

}
