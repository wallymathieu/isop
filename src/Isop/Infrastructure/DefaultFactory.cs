using System;
using System.Collections.Generic;
using System.Linq;
namespace Isop.Infrastructure
{
    public class DefaultFactory : IDisposable
    {
        private readonly IList<Object> _instances;

        public DefaultFactory()
        {
            _instances = new List<Object>();
        }

        public void Dispose()
        {
            foreach (var disposable in _instances
                .Where(i=>i is IDisposable)
                .Cast<IDisposable>())
            {
                disposable.Dispose();
            }
        }

        public Object Create(Type t)
        {
            var i = Activator.CreateInstance(t);
            _instances.Add(i);
            return i;
        }
    }
}

