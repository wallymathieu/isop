using System.Collections.Generic;
using System.IO;
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

        public void Invoke(TextWriter output) => InvokeAsync(output).Wait();

        public async Task InvokeAsync(TextWriter output)
        {
            var result = await _argumentInvoker.Invoke(_parsedArguments);
            foreach (var item in result)
            {
                output.Write(_formatter(item));
            }
        }
    }
}