using System;
using System.Collections.Generic;

namespace Isop.Domain
{
    public class TypeContainer
    {     
        public Dictionary<Type,Object> Instances { get; set; }
        private readonly Configuration _configuration;
        public TypeContainer (Configuration configuration)
        {
            Instances = new Dictionary<Type, Object>();
            _configuration = configuration;
        }
        public Object CreateInstance(Type type)
        {
            if (!Instances.ContainsKey(type))
            {
                var factory = _configuration.Factory;
                return factory(type);
            }
            return Instances[type];
        }
    }
}

