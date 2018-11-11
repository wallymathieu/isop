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
        public Parameter(ParameterInfo parameter)
        {
            _parameter = parameter;
        }
        public bool IsClassAndNotString()
        {
            var t = _parameter.ParameterType;
            return t.GetTypeInfo().IsClass && t != typeof(String);
        }
        public string Name{get{ return _parameter.Name;}}
        public Type ParameterType{get{ return _parameter.ParameterType;}}
        public bool LooksRequired()
        {
            return ! _parameter.IsOptional;
        }
        public IEnumerable<PropertyInfo> GetPublicInstanceProperties()
        {
            return _parameter.ParameterType.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }
        public object DefaultValue{get{ return _parameter.DefaultValue; }}

        public bool IsFile()
        {
            return _parameter.ParameterType == typeof(FileStream);
        }
    }
}

