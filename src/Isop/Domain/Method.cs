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
    public class Method(MethodInfo methodInfo)
    {
        public string Name => methodInfo.Name;
        public MethodInfo MethodInfo => methodInfo;
        public Parameter[] GetParameters() => methodInfo.GetParameters().Select(p => new Parameter(p)).ToArray();
        public object Invoke(object instance, object[] values)
        {
            if (instance==null) throw new ArgumentNullException(nameof(instance));
            try
            {
                return methodInfo.Invoke(instance, values);
            }catch(TargetInvocationException ex){
                if (ex.InnerException != null)
                {
                    ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                }
                throw;
            }
        }
        public IEnumerable<Argument> GetArguments(IFormatProvider? cultureInfo)
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

        private static Argument CreateArgument(Parameter parameterInfo, IFormatProvider? cultureInfo) =>
            new Argument(Required: parameterInfo.LooksRequired(),
                Parameter: ArgumentParameter.Parse(parameterInfo.Name, cultureInfo));

        private static IEnumerable<Argument> CreateArgumentsForInstanceProperties(Parameter parameterInfo, IFormatProvider cultureInfo) =>
            parameterInfo.GetPublicInstanceProperties()
                .Select(prop => 
                    new Argument( 
                        Required: parameterInfo.LooksRequired() && IsRequired(prop),
                        Parameter: ArgumentParameter.Parse(prop.Name, cultureInfo)));

        private static bool IsRequired(PropertyInfo propertyInfo) => 
            propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), true).Any();
    }
}

