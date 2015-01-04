using System;
using System.Collections.Generic;

namespace Isop.Controllers
{
    public class TypeContainer
    {
        public Func<Type,Object> Factory{get;set;}
     
        public Dictionary<Type,Object> Instances { get; set; }
        
        public TypeContainer ()
        {
            Instances = new Dictionary<Type, Object>();
        }
        public Object CreateInstance(Type type)
        {
            if (!Instances.ContainsKey(type))
            {
                var factory = Factory ??Activator.CreateInstance;
                return factory(type);
            }
            return Instances[type];
        }
    }
}

