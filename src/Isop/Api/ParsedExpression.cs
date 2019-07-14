using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Isop.Abstractions;
using Isop.CommandLine;
using Isop.CommandLine.Parse;
using Isop.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Isop.Api
{
    public class ParsedExpression
    {
        private readonly ParsedArguments _parsedArguments;
        private readonly AppHost _appHost;
        private ArgumentInvoker _argumentInvoker;
        private Formatter _formatter;
        public IEnumerable<RecognizedArgument> RecognizedArguments => _parsedArguments.RecognizedArguments;

        public IEnumerable<UnrecognizedArgument> UnRecognizedArguments => _parsedArguments.UnRecognizedArguments;

        public IEnumerable<Argument> ArgumentWithOptions => _parsedArguments.ArgumentWithOptions;

        public ParsedExpression(ParsedArguments parsedArguments, AppHost appHost)
        {
            _parsedArguments = parsedArguments;
            _appHost = appHost;
            _argumentInvoker = new ArgumentInvoker(_appHost.ServiceProvider);
            _formatter = _appHost.ServiceProvider.GetRequiredService<Formatter>();
        }

        public void Invoke(TextWriter output)
        {
            try
            {
                InvokeAsync(output).Wait();
            }
            catch (AggregateException e)
            {
                if (e.InnerException!=null && e.InnerExceptions.Count==1)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                }
                throw;
            }
        }

        public async Task InvokeAsync(TextWriter output)
        {
            var result = await _argumentInvoker.Invoke(_parsedArguments);
            foreach (var item in result)
            {
                var formatted = _formatter.Invoke(item);
                foreach (var str in formatted)
                {
                    await output.WriteLineAsync(str);
                }
            }
        }
    }
}