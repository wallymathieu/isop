﻿using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Isop.CommandLine;
using Isop.CommandLine.Parse;

namespace Isop.Domain
{
    public class Method
    {
        private readonly MethodInfo _methodInfo;

        public Method(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
        }
        public string Name => _methodInfo.Name;
        public MethodInfo MethodInfo => _methodInfo;
        public Controller Controller{ get; internal set;}
        public Parameter[] GetParameters() => _methodInfo.GetParameters().Select(p => new Parameter(p)).ToArray();
        public async Task<IEnumerable> Invoke(object instance, object[] values){
            if (instance==null) throw new ArgumentNullException(nameof(instance));
            try
            {
                var result = _methodInfo.Invoke(instance, values);
                if (result is Task task)
                {
                    var taskResult = await RunTask(task);
                    if (taskResult is IEnumerable taskEnumerable)
                        return taskEnumerable;
                    return new []{taskResult};
                }
                if (result is IEnumerable enumerable)
                    return enumerable;
                return new []{result};
            }catch(TargetInvocationException ex){
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return null;
            }
        }
        async Task<object> RunTask(Task task)
        {
            await task;
            var type = task.GetType();
            if (!type.IsGenericType)
            {
                return null;
            }
            return type.GetProperty("Result")?.GetValue(task) ;
        }

        public IEnumerable<Argument> GetArguments(IFormatProvider cultureInfo)
        {
            var parameterInfos = GetParameters();
            var recognizers = new List<Argument>();
            foreach (var parameterInfo in parameterInfos)
            {
                if (parameterInfo.IsClassAndNotString() && !parameterInfo.IsFile())
                {
                    AddArgumentWithOptionsForPropertiesOnObject(recognizers, parameterInfo, cultureInfo);
                }
                else
                {
                    var arg = GetArgumentWithOptions(parameterInfo, cultureInfo);
                    recognizers.Add(arg);
                }
            }
            return recognizers;
        }

        private static Argument GetArgumentWithOptions(Parameter parameterInfo, IFormatProvider cultureInfo)
        {
            return new Argument(required: parameterInfo.LooksRequired(),
                parameter: ArgumentParameter.Parse(parameterInfo.Name, cultureInfo));
        }

        private static void AddArgumentWithOptionsForPropertiesOnObject(List<Argument> recognizers,
            Parameter parameterInfo, IFormatProvider cultureInfo)
        {
            recognizers.AddRange(parameterInfo.GetPublicInstanceProperties()
                .Select(prop => 
                    new Argument( 
                        required: parameterInfo.LooksRequired() && IsRequired(prop),
                        parameter: ArgumentParameter.Parse(parameterInfo.Name, cultureInfo))));
        }
        public static bool IsRequired(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), true).Any();
        }
    }
}

