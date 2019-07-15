using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Isop.Implementations
{
    using Abstractions;
    using CommandLine;
    using CommandLine.Parse;

    internal class ParsedExpression:IParsedExpression
    {
        private readonly ParsedArguments _parsedArguments;
        private readonly AppHost _appHost;
        private readonly ArgumentInvoker _argumentInvoker;
        public IReadOnlyCollection<RecognizedArgument> Recognized => _parsedArguments.Recognized;

        public IReadOnlyCollection<UnrecognizedArgument> Unrecognized => _parsedArguments.Unrecognized;

        public IReadOnlyCollection<Argument> GlobalArguments => _parsedArguments.GlobalArguments;

        public ParsedExpression(ParsedArguments parsedArguments, AppHost appHost)
        {
            _parsedArguments = parsedArguments;
            _appHost = appHost;
            _argumentInvoker = new ArgumentInvoker(_appHost.ServiceProvider, _appHost.Recognizes, _appHost.HelpController);
        }

        public async Task InvokeAsync(TextWriter output)
        {
            var result = await _argumentInvoker.Invoke(_parsedArguments);
            foreach (var item in result)
            {
                var formatted = _appHost.Formatter.Invoke(item);
                foreach (var str in formatted) await output.WriteLineAsync(str);
            }
        }
    }
}