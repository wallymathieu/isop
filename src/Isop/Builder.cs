using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Isop.Abstractions;
using Isop.Api;
using Isop.CommandLine.Help;
using Isop.Domain;
using Isop.Help;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Isop
{
    /// <summary>
    /// Builder that uses 
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// 
        /// </summary>
        private class RecognizesConfigurationBuilder
        {
            /// <summary>
            /// 
            /// </summary>
            public RecognizesConfigurationBuilder()
            {
                Recognizes = new List<Controller>();
                Properties = new List<Property>();
            }
            /// <summary>
            /// 
            /// </summary>
            public IList<Controller> Recognizes { get; }
            /// <summary>
            /// 
            /// </summary>
            public IList<Property> Properties { get; }
        }
        private readonly RecognizesConfigurationBuilder _recognizes;
        private readonly IServiceCollection _container;


        /// <summary>
        /// Create an instance of builder
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static Builder Create(Configuration configuration=null)
        {
            return Create(configuration: configuration, serviceCollection: null);
        }
        /// <summary>
        /// Create an instance of builder
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static Builder Create(IServiceCollection serviceCollection, Configuration configuration=null)
        {
            if (serviceCollection == null)
            {
                serviceCollection = new ServiceCollection();
            }
            serviceCollection.AddOptions();
            serviceCollection.TryAddSingleton<Formatter>(di=>new ToStringFormatter().Format);
            serviceCollection.TryAddSingleton<TypeConverter>(di=>new DefaultConverter().ConvertFrom);
            serviceCollection.TryAddSingleton<HelpXmlDocumentation>();
            serviceCollection.TryAddSingleton<HelpForControllers>();
            serviceCollection.TryAddSingleton<HelpForArgumentWithOptions>();
            serviceCollection.TryAddSingleton<HelpController>();
            if (configuration != null)
            {
                serviceCollection.AddSingleton(Options.Create(configuration));
            }
            var recognizes = new RecognizesConfigurationBuilder();
            serviceCollection.TryAddSingleton(di=>new Recognizes(
                recognizes.Recognizes.ToArray(), 
                recognizes.Properties.ToArray()));
            recognizes.Recognizes.Add(new Controller(
                ignoreGlobalUnMatchedParameters: true, 
                type: typeof(HelpController)));
            return new Builder(serviceCollection, recognizes);
        }
        /// <summary>
        /// 
        /// </summary>
        private Builder(IServiceCollection container, RecognizesConfigurationBuilder recognizes)
        {
            _container = container;
            _recognizes = recognizes;
        }
        /// <summary>
        /// 
        /// </summary>
        public Builder SetTypeConverter(TypeConverter typeConverterFunc)
        {
            _container.AddSingleton(typeConverterFunc);
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        public Builder SetFormatter(Formatter formatter)
        {
            _container.AddSingleton(formatter);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="action"></param>
        /// <param name="required"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public Builder Parameter(string argument, ArgumentAction action = null, bool required = false, string description = null)
        {
            _recognizes.Properties.Add(new Property(argument, action, required, description, typeof(string)));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="action"></param>
        /// <param name="required"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public Builder Parameter(string argument, Action<string> action, bool required = false, string description = null)
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Builder FormatObjectsAsTable() => SetFormatter(new TableFormatter().Format);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="ignoreGlobalUnMatchedParameters"></param>
        /// <returns></returns>
        public Builder Recognize(Type arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            _container.TryAddSingleton(arg);
            _recognizes.Recognizes.Add(new Controller(arg, ignoreGlobalUnMatchedParameters));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="ignoreGlobalUnMatchedParameters"></param>
        /// <returns></returns>
        public Builder Recognize(object arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            var type = arg.GetType();
            _container.TryAddSingleton(type, svc => arg);
            _recognizes.Recognizes.Add(new Controller(type, ignoreGlobalUnMatchedParameters));
            return this;
        }

        /// <summary>
        /// Build instance of app host.
        /// </summary>
        public AppHost BuildAppHost()
        {
            var svcProvider= _container.BuildServiceProvider();
            var options = svcProvider.GetRequiredService<IOptions<Configuration>>();
            return new AppHost(options, svcProvider, svcProvider.GetRequiredService<Recognizes>());
        }
        /// <summary>
        /// Configure texts
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Builder WithLocalization(Action<Localization.Texts> action)
        {
            var t= new Localization.Texts();
            action(t);
            _container.AddSingleton(Options.Create(t));
            return this;
        }
    }
}
