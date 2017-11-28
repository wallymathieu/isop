using System;
using System.Collections.Generic;
using System.Linq;

namespace Isop.Domain
{
    public class TypeContainer
    {     
        private readonly Dictionary<Type,Object> _instances;
        private readonly Configuration _configuration;
        public TypeContainer (Configuration configuration)
        {
            _instances = new Dictionary<Type, Object>();
            _configuration = configuration;
        }

        public Object CreateInstance(Type type)
        {
            if (!_instances.ContainsKey(type))
            {
                var factory = _configuration.Factory;
                var instance = factory(type);
                if (null==instance)throw new NullReferenceException(type.FullName);
                _instances.Add(type,instance);
            }
            return _instances[type];
        }

        public void Add(Object instance)
        {
            _instances.Add(instance.GetType(), instance);
        }
    }
}

