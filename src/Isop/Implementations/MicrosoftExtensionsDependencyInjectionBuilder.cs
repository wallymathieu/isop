using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Isop.Abstractions;
using Isop.Domain;
using Isop.CommandLine.Parse;

namespace Isop.Implementations;

internal sealed class MicrosoftExtensionsDependencyInjectionBuilder(
    IServiceCollection serviceCollection,
    RecognizesBuilder recognizes) : IAppHostBuilder
{
    private Abstractions.ToStrings _toStrings = CommandLine.ToStrings.Default;
    private TypeConverter _typeConverter = DefaultConverter.ConvertFrom;

    public IAppHostBuilder SetTypeConverter(TypeConverter typeConverter)
    {
        _typeConverter = typeConverter;
        return this;
    }
    public IAppHostBuilder SetFormatter(Abstractions.ToStrings toStrings)
    {
        _toStrings = toStrings;
        return this;
    }
    public IAppHostBuilder Parameter(string argument, ArgumentAction? action = null, bool required = false, string? description = null)
    {
        return Parameter(
            argument: ArgumentParameter.Parse(argument, null),
            action: action,
            required: required,
            description: description);
    }
    public IAppHostBuilder Parameter(ArgumentParameter argument, ArgumentAction? action = null, bool required = false, string? description = null)
    {
        recognizes.Properties.Add(new ArgumentWithAction(argument, action, required, description));
        return this;
    }
    public IAppHostBuilder Parameter(string argument, Action<string?> action, bool required = false, string? description = null)
    {
        return Parameter(
            argument: ArgumentParameter.Parse(argument, null),
            action: action,
            required: required,
            description: description);
    }
    public IAppHostBuilder Parameter(ArgumentParameter argument, Action<string?> action, bool required = false, string? description = null)
    {
        var argumentAction = action != null
            ? new ArgumentAction(value =>
            {
                action(value);
                return Task.FromResult<object?>(null);
            })
            : null;
        recognizes.Properties.Add(new ArgumentWithAction(argument, argumentAction, required, description));
        return this;
    }

    public IAppHostBuilder Recognize(Type arg)
    {
        serviceCollection.TryAddSingleton(arg);
        recognizes.Recognizes.Add(new Domain.Controller(arg));
        return this;
    }
    public IAppHost BuildAppHost()
    {
        var svcProvider = serviceCollection.BuildServiceProvider();
        var options = svcProvider.GetService<IOptions<AppHostConfiguration>>();
        var conventions = svcProvider.GetService<IOptions<Conventions>>();
        var texts = svcProvider.GetService<IOptions<Localization.Texts>>();
        return new AppHost(options,
            svcProvider,
            new Recognizes(recognizes.Recognizes.ToArray(), recognizes.Properties.ToArray()),
            _typeConverter,
            _toStrings,
            texts,
            conventions);
    }
}
