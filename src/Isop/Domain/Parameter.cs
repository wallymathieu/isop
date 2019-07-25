using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace Isop.Domain
{
    using Infrastructure;
    public class Parameter
    {
        private readonly ParameterInfo _parameter;
        public Parameter(ParameterInfo parameter) => _parameter = parameter;

        public bool IsClassAndNotString() => 
            _parameter.ParameterType.GetTypeInfo().IsClass && _parameter.ParameterType != typeof(string);

        public string Name => _parameter.Name;
        public Type ParameterType => _parameter.ParameterType;

        public bool LooksRequired() => ! _parameter.IsOptional;

        public IEnumerable<PropertyInfo> GetPublicInstanceProperties() => 
            _parameter.ParameterType.GetTypeInfo()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);
        
        public object DefaultValue => _parameter.DefaultValue;

        public bool IsFile() => _parameter.ParameterType == typeof(FileStream);

        internal bool HasDefaultValue() => _parameter.HasDefaultValue;
    }
}

