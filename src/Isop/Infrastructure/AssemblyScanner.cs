using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Isop.Infrastructure
{
    public class AssemblyScanner
    {
        private readonly Assembly _assembly;
        private readonly Conventions _conventions;
        public AssemblyScanner (Assembly assembly)
        {
            _assembly = assembly;
            _conventions = new Conventions();
        }

        public IEnumerable<Type> LooksLikeControllers()
        {
            return _assembly.GetTypes().Where(t=>
                t.IsPublic
                && t.Name.EndsWithIC(_conventions.ControllerName) 
                && t.GetConstructors().Any(ctor=>ctor.GetParameters().Length==0)
                );
        }

        public IEnumerable<Object> IsopConfigurations()
        {
            return _assembly.GetTypes()
                .Where(type => type.Name.EqualsIC(_conventions.ConfigurationName));
        }
    }
}
