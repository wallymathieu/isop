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
        public IReadOnlyCollection<RecognizedArgument> Recognized
        {
            get
            {
                IReadOnlyCollection<RecognizedArgument> Recognized(ParsedArguments parsedArguments)
                {
                    return parsedArguments.Select(
                        properties: properties => properties.Recognized,
                        merged: merged => Recognized(merged.First).Union(Recognized(merged.Second)).ToArray(),
                        method: method => method.Recognized.ToArray(),
                        methodMissingArguments: methodMissingArgs=>new RecognizedArgument[0]
                    );
                }
                
                return Recognized(_parsedArguments);
            }
        }

        public IReadOnlyCollection<UnrecognizedArgument> Unrecognized {
            get
            {
                IReadOnlyCollection<UnrecognizedArgument> GetPotentiallyUnrecognized(ParsedArguments parsedArguments)
                {
                    return parsedArguments.Select(
                        properties: properties => properties.Unrecognized,
                        merged: merged => GetPotentiallyUnrecognized(merged.First).Union(GetPotentiallyUnrecognized(merged.Second)).ToArray(),
                        method: method => new UnrecognizedArgument[0],
                        methodMissingArguments: missingArgs=>new UnrecognizedArgument[0]
                    );
                }

                var potentiallyUnrecognized = GetPotentiallyUnrecognized(_parsedArguments);
                var recognized = new HashSet<int>(Recognized.SelectMany(r=>r.Index)); 
                return potentiallyUnrecognized.Where(unrecognized => !recognized.Contains(unrecognized.Index)).ToArray();
            }
        }
        
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