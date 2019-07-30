using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Isop.Implementations
{
    using Abstractions;
    using CommandLine;
    using Domain;
    internal class MicrosoftExtensionsDependencyInjectionBuilder : IAppHostBuilder
    {
        private readonly RecognizesBuilder _recognizes;
        private readonly IServiceCollection _serviceCollection;
        private Abstractions.ToStrings _toStrings;
        private TypeConverter _typeConverter;
       
        public MicrosoftExtensionsDependencyInjectionBuilder(IServiceCollection serviceCollection, RecognizesBuilder recognizes)
        {
            _serviceCollection = serviceCollection;
            _recognizes = recognizes;
            _toStrings = CommandLine.ToStrings.Default;
            _typeConverter = new DefaultConverter().ConvertFrom;
        }
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
            _serviceCollection.TryAddSingleton(arg);
            _recognizes.Recognizes.Add(new Domain.Controller(arg));
            return this;
        }
        public IAppHost BuildAppHost()
        {
            var svcProvider= _serviceCollection.BuildServiceProvider();
            var options = svcProvider.GetService<IOptions<Configuration>>();
            var conventions = svcProvider.GetService<IOptions<Conventions>>();
            var texts = svcProvider.GetService<IOptions<Localization.Texts>>();
            return new AppHost(options, 
                svcProvider, 
                new Recognizes(_recognizes.Recognizes.ToArray(), _recognizes.Properties.ToArray()),
                _typeConverter,
                _toStrings, 
                texts,
                conventions);
        }
    }
}