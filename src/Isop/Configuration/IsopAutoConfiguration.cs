using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Isop.Infrastructure;

namespace Isop.Configurations
{
    public class IsopAutoConfiguration
    {
        private Assembly _assembly;
        public IsopAutoConfiguration (Assembly assembly)
        {
            _assembly = assembly;
        }
        public IEnumerable<Type> Recognizes()
        {
            return _assembly.GetTypes().Where(t=>
                t.IsPublic
                && t.Name.EndsWithIC("controller") 
                && t.GetConstructors().Any(ctor=>ctor.GetParameters().Length==0)
                );
        }
        public void AddToConfiguration(Build build)
        {
            foreach (var item in Recognizes()) {
                build.Recognize(item);
            }
        }
    }
}