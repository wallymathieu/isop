using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Isop.Server.AspNet.Controllers
{
    public class ModelController : ApiController
    {
        private readonly IIsopServer _data;
        public ModelController(IIsopServer data)
        {
            _data = data;
        }

        [HttpGet]
        public Models.MethodTreeModel Get() 
        {
            return _data.GetModel();
        }
    }
}
