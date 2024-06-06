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
    using System.Threading;
    using System.Reflection;

    public class ArgumentInvoker(IServiceProvider serviceProvider, Recognizes recognizes, HelpController helpController)
    {
        private ILookup<string, ArgumentAction>? _recognizesMap;
        
        private ILookup<string,ArgumentAction> RecognizesMap =>
            _recognizesMap ??= recognizes.Properties.Where(p=>p.Action!=null).ToLookup(p=>p.Name, p=>p.Action);

#if NET8_0_OR_GREATER
        private static bool IsIAsyncEnumerable(Type type) =>(
                type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>));
        
        private static readonly MethodInfo _readAsyncEnumerableMethod = typeof(ArgumentInvoker).GetMethods(BindingFlags.Static|BindingFlags.NonPublic).Single(m => m.Name.Equals(nameof(ReadAsyncEnumerable)));
        private static async IAsyncEnumerable<object?> ReadAsyncEnumerable<T>(IAsyncEnumerable<T> enumerable)
        {
            await foreach (var val in enumerable)
            {
                yield return val;
            }
        }
        private static bool AsyncEnumerable(object result, out IAsyncEnumerable<object?>? enumerable)
        {
            enumerable = null;
            if (result is null) return false;
            Type t= result.GetType();
            if (IsIAsyncEnumerable(t)
                || t.GetInterfaces().Any(IsIAsyncEnumerable))
            {
                var argT = t.GetInterface("IAsyncEnumerable`1")!.GetGenericArguments();
                enumerable = (IAsyncEnumerable<object?>)_readAsyncEnumerableMethod.MakeGenericMethod(argT).Invoke(null, [result])!;
                return true;
            }
            return false;
        }
        #endif

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
                                    
                                InvokeResult? res;
                                if (result is Task task)
                                {
                                    res= new InvokeResult.AsyncControllerAction(RunTask(task));
                                }
                                #if NET8_0_OR_GREATER
                                else if (AsyncEnumerable(result,out var enumerable))
                                {
                                    res= new InvokeResult.ControllerAction(enumerable.ToBlockingEnumerable());
                                }
                                #endif
                                else
                                {
                                    res = new InvokeResult.ControllerAction(result);
                                }

                                return [ 
                                    Task.FromResult( res)];
                            });
                return tasks;
            }
        }
    }
}