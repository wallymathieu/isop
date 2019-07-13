using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace Isop
{
    using Infrastructure;
    using Domain;
    using Api;
    using Isop.Help;
    using CommandLine.Help;

    public class Build
    {
        public static Builder Create(Configuration configuration=null)
        {
            return Create(configuration: configuration, serviceCollection: null);
        }
        public static Builder Create(IServiceCollection serviceCollection, Configuration configuration=null)
        {
            if (serviceCollection == null)
            {
                serviceCollection = new ServiceCollection();
            }
            serviceCollection.AddOptions();
            serviceCollection.TryAddSingleton(new Formatter(new ToStringFormatter().Format));
            serviceCollection.TryAddSingleton(new TypeConverterFunc(new DefaultConverter().ConvertFrom));
            serviceCollection.TryAddSingleton<GlobalArguments>();
            serviceCollection.TryAddSingleton<HelpXmlDocumentation>();
            serviceCollection.TryAddSingleton<HelpForControllers>();
            serviceCollection.TryAddSingleton<HelpForArgumentWithOptions>();
            serviceCollection.TryAddSingleton<HelpController>();
            if (configuration != null)
            {
                serviceCollection.AddSingleton(Options.Create(configuration));
            }
            var recognizes = new RecognizesConfigurationBuilder();
            serviceCollection.TryAddSingleton(di=>new RecognizesConfiguration(
                recognizes.Recognizes.ToArray(), 
                recognizes.Properties.ToArray()));
            recognizes.Recognizes.Add(new Controller(
                ignoreGlobalUnMatchedParameters: true, 
                type: typeof(HelpController)));
            return new Builder(serviceCollection, recognizes);
        }
    }
}
