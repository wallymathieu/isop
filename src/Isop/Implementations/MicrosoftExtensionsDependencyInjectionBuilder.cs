using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Isop.Implementations
{
    using Abstractions;
    using CommandLine.Views;
    using Domain;
    internal class MicrosoftExtensionsDependencyInjectionBuilder : IAppHostBuilder
    {
        private readonly RecognizesBuilder _recognizes;
        private readonly IServiceCollection _serviceCollection;
        private Formatter _formatter;
        private TypeConverter _typeConverter;
       
        public MicrosoftExtensionsDependencyInjectionBuilder(IServiceCollection serviceCollection, RecognizesBuilder recognizes)
        {
            _serviceCollection = serviceCollection;
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

        public IAppHostBuilder Recognize(Type arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            _serviceCollection.TryAddSingleton(arg);
            _recognizes.Recognizes.Add(new Controller(arg, ignoreGlobalUnMatchedParameters));
            return this;
        }
        public IAppHostBuilder Recognize(object arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            var type = arg.GetType();
            _serviceCollection.TryAddSingleton(type, svc => arg);
            _recognizes.Recognizes.Add(new Controller(type, ignoreGlobalUnMatchedParameters));
            return this;
        }
        public IAppHost BuildAppHost()
        {
            var svcProvider= _serviceCollection.BuildServiceProvider();
            var options = svcProvider.GetService<IOptions<Configuration>>();
            return new AppHost(options, 
                svcProvider, 
                new Recognizes(this._recognizes.Recognizes.ToArray(), this._recognizes.Properties.ToArray()),
                _typeConverter,
                _formatter, 
                svcProvider.GetService<IOptions<Localization.Texts>>());
        }
    }
}