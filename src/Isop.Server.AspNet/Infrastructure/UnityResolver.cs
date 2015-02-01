using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Dependencies;
using Microsoft.Practices.Unity;
namespace Isop.Server.AspNet.Infrastructure
{
    class UnityResolver : IDependencyScope
    {
        private IUnityContainer child;

        public UnityResolver(IUnityContainer child)
        {
            this.child = child;
        }
        public object GetService(Type serviceType)
        {
            return child.Resolve(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return child.ResolveAll(serviceType);
        }

        public void Dispose()
        {
            child.Dispose();
        }
    }
}
