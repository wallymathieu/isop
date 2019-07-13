using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
namespace Isop
{
    using Infrastructure;
    using Domain;
    using Api;
    /// <summary>
    /// 
    /// </summary>
    public partial class Builder
    {
        private readonly RecognizesConfigurationBuilder _recognizes;
        /// <summary>
        /// 
        /// </summary>
        public Builder(IServiceCollection container, RecognizesConfigurationBuilder recognizes)
        {
            Container = container;
            _recognizes = recognizes;
        }
        /// <summary>
        /// 
        /// </summary>
        public Builder SetTypeConverter(TypeConverterFunc typeConverterFunc)
        {
            Container.AddSingleton(typeConverterFunc);
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        public Builder SetFormatter(Formatter formatter)
        {
            Container.AddSingleton(formatter);
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
        public Builder Parameter(string argument, Action<string> action = null, bool required = false, string description = null)
        {
            _recognizes.Properties.Add(new Property(argument, action, required, description, typeof(string)));
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
            Container.TryAddSingleton(arg);
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
            Container.TryAddSingleton(type, svc => arg);
            _recognizes.Recognizes.Add(new Controller(type, ignoreGlobalUnMatchedParameters));
            return this;
        }

        private IServiceCollection Container { get; }

        /// <summary>
        /// Build instance of app host.
        /// </summary>
        public AppHost BuildAppHost()
        {
            var svcProvider= Container.BuildServiceProvider();
            var options = svcProvider.GetRequiredService<IOptions<Configuration>>();
            return new AppHost(options, svcProvider, svcProvider.GetRequiredService<RecognizesConfiguration>());
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
            Container.AddSingleton(Options.Create(t));
            return this;
        }
    }
}
