using Isop.Server.AspNet.Infrastructure;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Isop.Server.AspNet
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var container = new UnityContainer();
            container.RegisterType<IIsopServer, IsopServerWithConfiguration>();
            GlobalConfiguration.Configuration.DependencyResolver = new DependencyResolver(container);
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
