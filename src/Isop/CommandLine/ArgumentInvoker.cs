using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Isop.CommandLine
{
    using Abstractions;
    using Parse;
    using Domain;
    using Isop.Help;
    
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

        public async Task<IEnumerable> Invoke(ParsedArguments parsedArguments)
        {
            IEnumerable<Task<object>> Choose(RecognizedArgument arg)
            {
                return RecognizesMap.Contains(arg.Argument.Name) 
                    ? RecognizesMap[arg.Argument.Name].Select(action => action(arg.Value)) 
                    : Enumerable.Empty<Task<object>>();
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                return await parsedArguments.Select<Task<IEnumerable>>(
                    @default: async args =>
                    {
                        var enumerable = args.Recognized
                            .SelectMany(Choose);
                        return await Task.WhenAll(enumerable);
                    },
                    merged: async merged => 
                        Join(await Invoke(merged.First), await Invoke(merged.Second)),
                    method: method =>
                    {
                        var instance = scope.ServiceProvider.GetService(method.RecognizedClass);
                        if (instance==null && method.RecognizedClass == typeof(HelpController))
                        {
                            instance = _helpController;
                        }
                        
                        if (ReferenceEquals(null, instance))
                            throw new Exception($"Unable to resolve {method.RecognizedClass.Name}");
                        return method.RecognizedAction.Invoke(instance,
                            method.RecognizedActionParameters.ToArray());
                    });
            }
        }

        private static IEnumerable Join(IEnumerable first, IEnumerable second)
        {
            foreach (var item in first)
            {
                yield return item;
            }
            foreach (var item in second)
            {
                yield return item;
            }
        }
    }
}