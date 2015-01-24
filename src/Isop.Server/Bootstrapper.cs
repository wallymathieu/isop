using Nancy;
using Nancy.Bootstrapper;
using Nancy.ViewEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.TinyIoc;
using Newtonsoft.Json;
namespace Isop.Server
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            // No registrations should be performed in here, however you may
            // resolve things that are needed during application startup.
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer existingContainer)
        {
            base.ConfigureApplicationContainer(existingContainer);
            // Perform registation that should have an application lifetime

            existingContainer
                .Register<IIsopServer, IsopServerFromAssemblyLocation>();

            existingContainer.Register<JsonSerializer, CustomJsonSerializer>();

        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            // Perform registrations that should have a request lifetime

            base.ConfigureRequestContainer(container, context);
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            // No registrations should be performed in here, however you may
            // resolve things that are needed during request startup.
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(c => c.ViewLocationProvider = typeof(ResourceViewLocationProvider));
            }
        }

        protected override IEnumerable<Type> ViewEngines
        {
            get
            {
                return new[] { typeof(Nancy.ViewEngines.Veil.VeilViewEngine) };
            }
        }
        //protected override void RequestStartup(ILifetimeScope requestContainer, IPipelines pipelines, NancyContext context)
        //{

        //    base.RequestStartup(requestContainer, pipelines, context);
        //}
    }
}
