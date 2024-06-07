using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Isop.Implementations
{
    using Abstractions;
    using CommandLine;
    using Domain;
    internal class ServiceProviderAppHostBuilder(IServiceProvider serviceProvider, RecognizesBuilder recognizes) : IAppHostBuilder
    {
        private Abstractions.ToStrings _toStrings = CommandLine.ToStrings.Default;
        private TypeConverter _typeConverter = new DefaultConverter().ConvertFrom;

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
            recognizes.Properties.Add(new Property(argument, action, required, description));
            return this;
        }
        public IAppHostBuilder Parameter(string argument, Action<string?> action, bool required = false, string? description = null)
        {
            var argumentAction = action!=null 
                ? new ArgumentAction(value=>
                {
                    action(value);
                    return Task.FromResult<object?>(null);
                })
                : null;
            recognizes.Properties.Add(new Property(argument, argumentAction, required, description));
            return this;
        }

        public IAppHostBuilder Recognize(Type arg)
        {
            recognizes.Recognizes.Add(new Domain.Controller(arg));
            return this;
        }
        /// <summary>
        /// Build instance of app host.
        /// </summary>
        public IAppHost BuildAppHost()
        {
            var options = serviceProvider.GetService<IOptions<Configuration>>();
            var conventions = serviceProvider.GetService<IOptions<Conventions>>();
            var texts = serviceProvider.GetService<IOptions<Localization.Texts>>();

            return new AppHost(options, 
                serviceProvider, 
                new Recognizes(recognizes.Recognizes.ToArray(), recognizes.Properties.ToArray()),
                _typeConverter,
                _toStrings, 
                texts,
                conventions);
        }
    }
}