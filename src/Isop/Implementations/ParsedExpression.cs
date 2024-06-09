using System.IO;
using System.Linq;
using Isop.Abstractions;
using Isop.CommandLine;
using Isop.CommandLine.Parse;

namespace Isop.Implementations;

internal sealed class ParsedExpression : IParsed
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
                    methodMissingArguments: methodMissingArgs => []
                );
            }

            return Recognized(_parsedArguments);
        }
    }

    public IReadOnlyCollection<UnrecognizedArgument> Unrecognized
    {
        get
        {
            IReadOnlyCollection<UnrecognizedArgument> GetPotentiallyUnrecognized(ParsedArguments parsedArguments)
            {
                return parsedArguments.Select(
                    properties: properties => properties.Unrecognized,
                    merged: merged => GetPotentiallyUnrecognized(merged.First).Union(GetPotentiallyUnrecognized(merged.Second)).ToArray(),
                    method: method => [],
                    methodMissingArguments: missingArgs => []
                );
            }

            var potentiallyUnrecognized = GetPotentiallyUnrecognized(_parsedArguments);
            var recognized = new HashSet<int>(Recognized.SelectMany(r => r.Index));
            return potentiallyUnrecognized.Where(unrecognized => !recognized.Contains(unrecognized.Index)).ToArray();
        }
    }

    public ParsedExpression(ParsedArguments parsedArguments, AppHost appHost)
    {
        _parsedArguments = parsedArguments;
        _appHost = appHost;
        _argumentInvoker = new ArgumentInvoker(_appHost.ServiceProvider, _appHost.Recognizes, _appHost.HelpController);
    }
    private IEnumerable<string> MissingRequiredArguments()
    {
        var missingRequiredArguments = _appHost.Recognizes.Properties
            .Where(property => property.Required)
            .Where(property => !Recognized
                .Any(recognizedArgument => recognizedArgument.Argument.Name.EqualsIgnoreCase(property.Name)))
            .Select(property => property.Name);

        static IReadOnlyCollection<string> GetMissing(ParsedArguments parsedArguments)
        {
            return parsedArguments.Select(
                properties: properties => [],
                merged: merged => GetMissing(merged.First).Union(GetMissing(merged.Second), StringComparer.OrdinalIgnoreCase).ToArray(),
                method: method => [],
                methodMissingArguments: missingArgs => missingArgs.MissingParameters
            );
        }
        return missingRequiredArguments.Union(GetMissing(_parsedArguments), StringComparer.OrdinalIgnoreCase);
    }

    private void AssertFailOnMissing()
    {
        var missing = MissingRequiredArguments().ToArray();
        if (missing.Length != 0)
            throw new MissingArgumentException("Missing arguments", missing);
    }
    public async Task InvokeAsync(TextWriter? output)
    {
        AssertFailOnMissing();
        output ??= Console.Out;
        foreach (var result in _argumentInvoker.Invoke(_parsedArguments))
        {
            var item = await result;
            var formatted = await item.Select(
                argument: arg => Task.FromResult(_appHost.ToStrings(arg.Result)),
                asyncControllerAction: async ca => _appHost.ToStrings(await ca.Task),
                controllerAction: ca => Task.FromResult(_appHost.ToStrings(ca.Result)),
                empty: e => Task.FromResult(Enumerable.Empty<string>())
            );
            foreach (var str in formatted) await output.WriteLineAsync(str);
        }
    }
    public async Task<IEnumerable<InvokeResult>> InvokeAsync()
    {
        AssertFailOnMissing();
        return await Task.WhenAll(_argumentInvoker.Invoke(_parsedArguments));
    }

    [Obsolete("Prefer HelpAsync")]
    public string Help() => _appHost.Help();
    public Task<string> HelpAsync() => _appHost.HelpAsync();
}
