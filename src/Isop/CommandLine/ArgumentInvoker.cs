using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isop.Abstractions;
using Isop.CommandLine.Parse;
using Isop.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Isop.CommandLine
{
    public class ArgumentInvoker
    {
        private IServiceProvider _serviceProvider;
        private Recognizes _recognizes;
        private ILookup<string, ArgumentAction> _recognizesMap;
        private Recognizes recognizes => _recognizes ??= _serviceProvider.GetRequiredService<Recognizes>();
        private ILookup<string,ArgumentAction> recognizesMap => _recognizesMap 
            ??= recognizes.Properties.Where(p=>p.Action!=null).ToLookup(p=>p.Name, p=>p.Action);

        public ArgumentInvoker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable> Invoke(ParsedArguments parsedArguments)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                return await parsedArguments.Map<Task<IEnumerable>>(
                    @default: async args =>
                    {
                        var enumerable = args.RecognizedArguments.SelectMany(arg => recognizesMap.Contains(arg.Argument.Name) 
                            ? recognizesMap[arg.Argument.Name].Select(action => action(arg.Value)) 
                            : Enumerable.Empty<Task<object>>());
                        return await Task.WhenAll(enumerable);
                    },
                    merged: async merged => 
                        Join(await Invoke(merged.First), await Invoke(merged.Second)),
                    method: method =>
                    {
                        var instance = scope.ServiceProvider.GetService(method.RecognizedClass);
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