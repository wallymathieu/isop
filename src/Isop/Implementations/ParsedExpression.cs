using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Isop.Infrastructure;

namespace Isop.Implementations
{
    using Abstractions;
    using CommandLine;
    using CommandLine.Parse;
    using Domain;

    internal class ParsedExpression:IParsedExpression
    {
        private readonly ParsedArguments _parsedArguments;
        private readonly AppHost _appHost;
        private readonly ArgumentInvoker _argumentInvoker;
        public IReadOnlyCollection<RecognizedArgument> Recognized => _parsedArguments.Recognized;

        public IReadOnlyCollection<UnrecognizedArgument> Unrecognized => _parsedArguments.Unrecognized;
        
        public ParsedExpression(ParsedArguments parsedArguments, AppHost appHost)
        {
            _parsedArguments = parsedArguments;
            _appHost = appHost;
            _argumentInvoker = new ArgumentInvoker(_appHost.ServiceProvider, _appHost.Recognizes, _appHost.HelpController);
        }
        private IEnumerable<string> UnMatchedRequiredArguments()
        {
            var unMatchedRequiredArguments = _appHost.Recognizes.Properties
                .Where(property => property.Required)
                .Where(property => !Recognized
                    .Any(recognizedArgument => recognizedArgument.Argument.Name.EqualsIgnoreCase(property.Name)))
                .Select(unmatched =>unmatched.Name);
            // TODO: Controller methods
            return unMatchedRequiredArguments;
        }

        private void AssertFailOnUnMatched()
        {
            var unMatchedRequiredArguments = UnMatchedRequiredArguments().ToArray();

            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments")
                {
                    Arguments = unMatchedRequiredArguments
                };
            }
        }
        public async Task InvokeAsync(TextWriter output)
        {
            AssertFailOnUnMatched();
            
            var result = await _argumentInvoker.Invoke(_parsedArguments);
            foreach (var item in result)
            {
                var formatted = _appHost.Formatter.Invoke(item);
                foreach (var str in formatted) await output.WriteLineAsync(str);
            }
        }
    }
}