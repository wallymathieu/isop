using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace Isop.Api
{
    using Infrastructure;
    using Domain;
    using Microsoft.Extensions.Options;

    public class Builder
    {
        private readonly RecognizesConfiguration recognizes;

        public Builder(IServiceCollection collection)
        {
            this.Container = collection;
            this.recognizes = new RecognizesConfiguration();
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
            Container.AddSingleton(new TableFormatter().Format);
            return this;
        }


        public Builder Recognize(Type arg, bool ignoreGlobalUnMatchedParameters = false)
        {
            Container.TryAddScoped(arg);
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

        public ParserWithConfiguration Build()
        {
            var svcProvider= Container.BuildServiceProvider();
            var options = svcProvider.GetRequiredService<IOptions<Configuration>>();
            return new ParserWithConfiguration(options, svcProvider, recognizes);
        }

        public Builder WithHelpTexts(Action<Help.HelpTexts> action)
        {
            var t= new Help.HelpTexts();
            action(t);
            Container.AddSingleton(Options.Create(t));
            return this;
        }
    }
}
