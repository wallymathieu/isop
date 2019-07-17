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
        
        public IReadOnlyCollection<Argument> PotentialArguments=>new Argument[0];//todo: Fix
        
        public ParsedExpression(ParsedArguments parsedArguments, AppHost appHost)
        {
            _parsedArguments = parsedArguments;
            _appHost = appHost;
            _argumentInvoker = new ArgumentInvoker(_appHost.ServiceProvider, _appHost.Recognizes, _appHost.HelpController);
        }
        private IEnumerable<Property> UnMatchedRequiredArguments()
        {
            var unMatchedRequiredArguments = _appHost.Recognizes.Properties
                .Where(argumentWithOptions => argumentWithOptions.Required)
                .Where(argumentWithOptions => !Recognized
                    .Any(recognizedArgument => recognizedArgument.Argument.Name.EqualsIgnoreCase(argumentWithOptions.Name)));
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
                        .Select(unmatched =>unmatched.Name)
                        .ToArray()
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