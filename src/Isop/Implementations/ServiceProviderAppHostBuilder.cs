using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Isop.Implementations
{
    using Abstractions;
    using CommandLine.Views;
    using Domain;
    internal class ServiceProviderAppHostBuilder : IAppHostBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RecognizesBuilder _recognizes;
        private Formatter _formatter;
        private TypeConverter _typeConverter;

        public ServiceProviderAppHostBuilder(IServiceProvider serviceProvider, RecognizesBuilder recognizes)
        {
            _serviceProvider = serviceProvider;
            _recognizes = recognizes;
            _formatter = new ToStringFormatter().Format;
            _typeConverter = new DefaultConverter().ConvertFrom;
        }
        public IAppHostBuilder SetTypeConverter(TypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
            return this;
        }
        public IAppHostBuilder SetFormatter(Formatter formatter)
        {
            _formatter = formatter;
            return this;
        }

        public IAppHostBuilder Parameter(string argument, ArgumentAction action = null, bool required = false, string description = null)
        {
            _recognizes.Properties.Add(new Property(argument, action, required, description));
            return this;
        }
        public IAppHostBuilder Parameter(string argument, Action<string> action, bool required = false, string description = null)
        {
            var argumentAction = action!=null 
                ? new ArgumentAction(value=>
                {
                    action(value);
                    return Task.FromResult<object>(null);
                })
                : null;
            _recognizes.Properties.Add(new Property(argument, argumentAction, required, description));
            return this;
        }

        public IAppHostBuilder Recognize(Type arg)
        {
            _recognizes.Recognizes.Add(new Controller(arg));
            return this;
        }
        /// <summary>
        /// Build instance of app host.
        /// </summary>
        public IAppHost BuildAppHost()
        {
            var options = _serviceProvider.GetService<IOptions<Configuration>>();
            var conventions = _serviceProvider.GetService<IOptions<Conventions>>();
            var texts = _serviceProvider.GetService<IOptions<Localization.Texts>>();

            return new AppHost(options, 
                _serviceProvider, 
                new Recognizes(_recognizes.Recognizes.ToArray(), _recognizes.Properties.ToArray()),
                _typeConverter,
                _formatter, 
                texts,
                conventions);
        }
    }
}