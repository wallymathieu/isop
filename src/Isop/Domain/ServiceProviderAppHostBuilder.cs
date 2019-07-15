using System;
using System.Linq;
using System.Threading.Tasks;
using Isop.Abstractions;
using Isop.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Isop.Domain
{
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
            _recognizes.Properties.Add(new Property(argument, action, required, description, typeof(string)));
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
            _recognizes.Properties.Add(new Property(argument, argumentAction, required, description, typeof(string)));
            return this;
        }

        public IAppHostBuilder Recognize(Type arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            _recognizes.Recognizes.Add(new Controller(arg, ignoreGlobalUnMatchedParameters));
            return this;
        }
        public IAppHostBuilder Recognize(object arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            var type = arg.GetType();
            _recognizes.Recognizes.Add(new Controller(type, ignoreGlobalUnMatchedParameters));
            return this;
        }

        /// <summary>
        /// Build instance of app host.
        /// </summary>
        public AppHost BuildAppHost()
        {
            var options = _serviceProvider.GetService<IOptions<Configuration>>();
            return new AppHost(options, 
                _serviceProvider, 
                new Recognizes(this._recognizes.Recognizes.ToArray(), this._recognizes.Properties.ToArray()),
                _typeConverter,
                _formatter, 
                _serviceProvider.GetService<IOptions<Localization.Texts>>());
        }
    }
}