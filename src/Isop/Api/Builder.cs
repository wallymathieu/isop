using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
namespace Isop.Api
{
    using Infrastructure;
    using Domain;
    using System.Linq;

    public class Builder
    {
        private readonly RecognizesConfigurationBuilder recognizes;

        public Builder(IServiceCollection container, RecognizesConfigurationBuilder recognizes)
        {
            this.Container = container;
            this.recognizes = recognizes;
        }

        public Builder SetTypeConverter(TypeConverterFunc typeConverterFunc)
        {
            Container.AddSingleton(typeConverterFunc);
            return this;
        }

        public Builder SetFormatter(Formatter formatter)
        {
            Container.AddSingleton(formatter);
            return this;
        }

        public void Add(Type item)
        {
            Recognize(item);
        }


        public Builder Parameter(string argument, Action<string> action = null, bool required = false, string description = null)
        {
            recognizes.Properties.Add(new Property(argument, action, required, description, typeof(string)));
            return this;
        }

        public Builder FormatObjectsAsTable()
        {
            Container.AddSingleton(new Formatter(new TableFormatter().Format));
            return this;
        }


        public Builder Recognize(Type arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            Container.TryAddSingleton(arg);
            recognizes.Recognizes.Add(new Controller(arg, ignoreGlobalUnMatchedParameters));
            return this;
        }
        public Builder Recognize(Object arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            var type = arg.GetType();
            Container.TryAddSingleton(type, svc => arg);
            recognizes.Recognizes.Add(new Controller(type, ignoreGlobalUnMatchedParameters));
            return this;
        }
        public IServiceCollection Container { get; }

        public AppHost Build()
        {
            var svcProvider= Container.BuildServiceProvider();
            var options = svcProvider.GetRequiredService<IOptions<Configuration>>();
            return new AppHost(options, svcProvider, svcProvider.GetRequiredService<RecognizesConfiguration>());
        }

        public Builder WithHelpTexts(Action<Localization.Texts> action)
        {
            var t= new Localization.Texts();
            action(t);
            Container.AddSingleton(Options.Create(t));
            return this;
        }
    }
}
