using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isop.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Isop.CommandLine
{
    using Abstractions;
    using Parse;
    using Domain;
    using Isop.Help;
    using Infrastructure;
    
    public class ArgumentInvoker(IServiceProvider serviceProvider, Recognizes recognizes, HelpController helpController)
    {
        private ILookup<string, ArgumentAction>? _recognizesMap;
        
        private ILookup<string,ArgumentAction> RecognizesMap =>
            _recognizesMap ??= recognizes.Properties.Where(p=>p.Action!=null).ToLookup(p=>p.Name, p=>p.Action);

        public IEnumerable<Task<InvokeResult>> Invoke(ParsedArguments parsedArguments)
        {
            IEnumerable<Task<InvokeResult>> ChooseArgument(RecognizedArgument arg)
            {
                return RecognizesMap.Contains(arg.Argument.Name) 
                    ? RecognizesMap[arg.Argument.Name].Select(async action => 
                        (InvokeResult)new InvokeResult.Argument( await action(arg.Value)))
                    : Enumerable.Empty<Task<InvokeResult>>();
            }
            async Task<object?> RunTask(Task task)
            {
                await task;
                var type = task.GetType();
                if (!type.IsGenericType)
                {
                    return null;
                }
                return type.GetProperty("Result")?.GetValue(task) ;
            }
            using (var scope = serviceProvider.CreateScope())
            {
                var tasks = parsedArguments.Select(
                    methodMissingArguments: empty=>new[]{Task.FromResult<InvokeResult>(new InvokeResult.Empty())},
                    properties: args =>
                    {
                        var enumerable = args.Recognized
                            .SelectMany(ChooseArgument);
                        return enumerable;
                    },
                    merged: merged => Invoke(merged.First).Concat(Invoke(merged.Second)),
                            method: method =>
                            {
                                var instance = scope.ServiceProvider.GetService(method.RecognizedClass);
                                if (instance==null && method.RecognizedClass == typeof(HelpController))
                                {
                                    instance = helpController;
                                }
                        
                                if (ReferenceEquals(null, instance))
                                    throw new Exception($"Unable to resolve {method.RecognizedClass.Name}");
                                var result = method.RecognizedAction.Invoke(instance,
                                    method.RecognizedActionParameters.ToArray());
                                InvokeResult? res = result switch
                                {
                                    Task task=> new InvokeResult.AsyncControllerAction(RunTask(task)),
                                    #if NET8_0_OR_GREATER
                                    System.Collections.Generic.IAsyncEnumerable<object> enumerable=>
                                         new InvokeResult.ControllerAction(enumerable.ToBlockingEnumerable()),
                                    #endif
                                    _ => new InvokeResult.ControllerAction(result),
                                };

                                return [ 
                                    Task.FromResult( res)];
                            });
                return tasks;
            }
        }
    }
}