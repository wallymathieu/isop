using Isop.Server.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace Isop.Server.AspNet.Controllers
{
    public class MethodController : ApiController
    {
        private readonly IIsopServer _data;
        public MethodController(IIsopServer data)
        {
            _data = data;
        }

        [HttpPost]
        public HttpResponseMessage Post(string controller, string method, FormDataCollection formData)
        {
            try
            {
                var cmethod = _data.GetControllerMethod(controller, method);
                var result = _data.InvokeMethod(cmethod, ToDictionary(formData));
                return Request.CreateResponse(HttpStatusCode.OK, result, Configuration.Formatters.JsonFormatter);
            }
            catch (TypeConversionFailedException e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ToModel(e), Configuration.Formatters.JsonFormatter);
            }
            catch (MissingArgumentException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ToModel(ex), Configuration.Formatters.JsonFormatter);
            }
        }

        private object ToModel(MissingArgumentException ex)
        {
            return ex.Arguments.Select(arg => new MissingArgument
            {
                Message = "Missing arguments.",
                Argument = arg
            }).ToArray();
        }

        private object ToModel(TypeConversionFailedException e)
        {
            return new[]{
                new TypeConversionFailed
                {
                    Message = "Type conversion failed.",
                    Argument = e.Argument,
                    Value = e.Value,
                    TargetType = e.TargetType.FullName
                }
            };
        }

        public static IDictionary<string, object> ToDictionary(FormDataCollection col)
        {
            return col.ToDictionary(p => p.Key, p => (object)p.Value);
        }
    }
}
