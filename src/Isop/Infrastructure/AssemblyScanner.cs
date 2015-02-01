using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Isop.Infrastructure
{
    using Domain;
    public class AssemblyScanner
    {
        private readonly Assembly _assembly;
        public AssemblyScanner(Assembly assembly)
        {
            _assembly = assembly;
        }

        public IEnumerable<Type> LooksLikeControllers()
        {
            return _assembly.GetTypes().Where(t =>
                t.IsPublic
                && t.Name.EndsWithIC(Conventions.ControllerName)
                && t.GetConstructors().Any(ctor => ctor.GetParameters().Length == 0)
                );
        }

        public IEnumerable<Object> IsopConfigurations()
        {
            return _assembly.GetTypes()
                .Where(type => Conventions.ConfigurationName.Contains(type.Name));
        }
    }
}
