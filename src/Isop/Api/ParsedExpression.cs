using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Isop.CommandLine;
using Isop.CommandLine.Parse;

namespace Isop.Api
{
    public class ParsedExpression
    {
        private readonly ParsedArguments _parsedArguments;
        private readonly AppHost _appHost;
        private readonly ArgumentInvoker _argumentInvoker;
        public IReadOnlyCollection<RecognizedArgument> RecognizedArguments => _parsedArguments.RecognizedArguments;

        public IReadOnlyCollection<UnrecognizedArgument> UnRecognizedArguments => _parsedArguments.UnRecognizedArguments;

        public IReadOnlyCollection<Argument> ArgumentWithOptions => _parsedArguments.ArgumentWithOptions;

        public ParsedExpression(ParsedArguments parsedArguments, AppHost appHost)
        {
            _parsedArguments = parsedArguments;
            _appHost = appHost;
            _argumentInvoker = new ArgumentInvoker(_appHost.ServiceProvider, _appHost.Recognizes, _appHost.HelpController);
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
                var formatted = _appHost.Formatter.Invoke(item);
                foreach (var str in formatted)
                {
                    await output.WriteLineAsync(str);
                }
            }
        }
    }
}