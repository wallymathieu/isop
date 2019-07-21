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
    
    public class ArgumentInvoker
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Recognizes _recognizes;
        private readonly HelpController _helpController;
        private ILookup<string, ArgumentAction> _recognizesMap;
        
        private ILookup<string,ArgumentAction> RecognizesMap => _recognizesMap 
            ??(_recognizesMap= _recognizes.Properties.Where(p=>p.Action!=null).ToLookup(p=>p.Name, p=>p.Action));

        public ArgumentInvoker(IServiceProvider serviceProvider, Recognizes recognizes, HelpController helpController)
        {
            _serviceProvider = serviceProvider;
            _recognizes = recognizes;
            _helpController = helpController;
        }

        public IEnumerable<Task<IResult>> Invoke(ParsedArguments parsedArguments)
        {
            IEnumerable<Task<IResult>> ChooseArgument(RecognizedArgument arg)
            {
                return RecognizesMap.Contains(arg.Argument.Name) 
                    ? RecognizesMap[arg.Argument.Name].Select(async action => 
                        (IResult)new Result.Argument( await action(arg.Value)))
                    : Enumerable.Empty<Task<IResult>>();
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
            using (var scope = _serviceProvider.CreateScope())
            {
                var tasks = parsedArguments.Select(
                    methodMissingArguments: empty=>new[]{Task.FromResult<IResult>(new Result.Empty())},
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
                                    instance = _helpController;
                                }
                        
                                if (ReferenceEquals(null, instance))
                                    throw new Exception($"Unable to resolve {method.RecognizedClass.Name}");
                                var result = method.RecognizedAction.Invoke(instance,
                                    method.RecognizedActionParameters.ToArray());
                                return new []{ Task.FromResult(result is Task task 
                                    ? (IResult)new Result.AsyncControllerAction(RunTask(task))
                                    : new Result.ControllerAction(result)) };
                            });
                return tasks;
            }
        }
    }
}