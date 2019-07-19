using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Isop.Domain
{
    using CommandLine;
    using CommandLine.Parse;
    public class Method
    {
        private readonly MethodInfo _methodInfo;
        public Method(MethodInfo methodInfo) => _methodInfo = methodInfo;
        public string Name => _methodInfo.Name;
        public MethodInfo MethodInfo => _methodInfo;
        public Parameter[] GetParameters() => _methodInfo.GetParameters().Select(p => new Parameter(p)).ToArray();
        public async Task<IEnumerable> Invoke(object instance, object[] values)
        {
            if (instance==null) throw new ArgumentNullException(nameof(instance));
            try
            {
                var result = _methodInfo.Invoke(instance, values);
                switch (result)
                {
                    case Task task:
                    {
                        var taskResult = await RunTask(task);
                        switch (taskResult)
                        {
                            case string _:
                                return new []{taskResult};
                            case IEnumerable taskEnumerable:
                                return taskEnumerable;
                            default:
                                return new []{taskResult};
                        }
                    }
                    case string _:
                        return new []{result};
                    case IEnumerable enumerable:
                        return enumerable;
                    default:
                        return new []{result};
                }
            }catch(TargetInvocationException ex){
                if (ex.InnerException != null)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }
                throw;
            }/*catch(AggregateException aex){ // perhaps?
                if (aex.InnerException != null && aex.InnerExceptions.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(aex.InnerException).Throw();
                }
                throw;
            }*/
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
                    recognizers.AddRange(CreateArgumentsForInstanceProperties(parameterInfo, cultureInfo));
                }
                else
                {
                    recognizers.Add(CreateArgument(parameterInfo, cultureInfo));
                }
            }
            return recognizers;
        }

        private static Argument CreateArgument(Parameter parameterInfo, IFormatProvider cultureInfo) =>
            new Argument(required: parameterInfo.LooksRequired(),
                parameter: ArgumentParameter.Parse(parameterInfo.Name, cultureInfo));

        private static IEnumerable<Argument> CreateArgumentsForInstanceProperties(Parameter parameterInfo, IFormatProvider cultureInfo) =>
            parameterInfo.GetPublicInstanceProperties()
                .Select(prop => 
                    new Argument( 
                        required: parameterInfo.LooksRequired() && IsRequired(prop),
                        parameter: ArgumentParameter.Parse(parameterInfo.Name, cultureInfo)));

        private static bool IsRequired(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), true).Any();
        }
    }
}

