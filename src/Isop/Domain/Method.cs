using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Isop.Domain
{
    public class Method
    {
        private readonly MethodInfo _methodInfo;

        public Method(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
        }
        public string Name{get{ return _methodInfo.Name;}}
        public MethodInfo MethodInfo{get{ return _methodInfo;}}
        public Controller Controller{ get; internal set;}
        public Type ReturnType{get{ return _methodInfo.ReturnType;}}
        public Parameter[] GetParameters(){
            return _methodInfo.GetParameters().Select(p => new Parameter(p)).ToArray();
        }
        public object Invoke(object instance, object[] values){
            return _methodInfo.Invoke(instance, values);
        }

        public IEnumerable<Argument> GetArguments()
        {
            var parameterInfos = GetParameters();
            var recognizers = new List<Argument>();
            foreach (var parameterInfo in parameterInfos)
            {
                if (parameterInfo.IsClassAndNotString() && !parameterInfo.IsFile())
                {
                    AddArgumentWithOptionsForPropertiesOnObject(recognizers, parameterInfo);
                }
                else
                {
                    var arg = GetArgumentWithOptions(parameterInfo);
                    recognizers.Add(arg);
                }
            }
            return recognizers;
        }

        private static Argument GetArgumentWithOptions(Parameter parameterInfo)
        {
            return new Argument(parameterInfo.Name,
                required: parameterInfo.LooksRequired(),
                type: parameterInfo.ParameterType);
        }

        private static void AddArgumentWithOptionsForPropertiesOnObject(List<Argument> recognizers, Parameter parameterInfo)
        {
            recognizers.AddRange(parameterInfo.GetPublicInstanceProperties()
                .Select(prop => 
                    new Argument(prop.Name, 
                        required: parameterInfo.LooksRequired() && IsRequired(prop), 
                        type: prop.PropertyType)));
        }
        public static bool IsRequired(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), true).Any();
        }
    }
}

