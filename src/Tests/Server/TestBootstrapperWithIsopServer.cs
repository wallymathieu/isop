using Nancy;
using Isop.Server;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace Isop.Wpf.Tests.Server
{
    public class TestBootstrapperWithIsopServer <TISopServer>: Bootstrapper 
        where TISopServer: class, IIsopServer
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer existingContainer)
        {
            base.ConfigureApplicationContainer(existingContainer);
            // Perform registation that should have an application lifetime
            existingContainer
                .Register<IIsopServer, TISopServer>();
        }
    }
}
