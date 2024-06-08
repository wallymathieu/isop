using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Isop.Abstractions;
using Isop.Help;
using Isop.Implementations;

namespace Isop
{
    /// <summary>
    /// An app host builder allows you to build a command line application host.
    /// This is to mimic model view controller pattern but using command line.
    /// </summary>
    public static class AppHostBuilder 
    {
        /// <summary>
        /// Create an instance of builder using IServiceCollection to wire up dependencies
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IAppHostBuilder Create(AppHostConfiguration configuration) => 
            Create(configuration: configuration, serviceCollection: null);

        /// <summary>
        /// Create an instance of builder that uses IServiceCollection to configure IServiceProvider
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IAppHostBuilder Create(IServiceCollection? serviceCollection, AppHostConfiguration? configuration=null)
        {
            serviceCollection ??= new ServiceCollection();
            serviceCollection.AddOptions();
            //
            if (configuration != null)
            {
                serviceCollection.AddSingleton(Options.Create(configuration));
            }
            var recognizes = new RecognizesBuilder();
            recognizes.Recognizes.Add(new Domain.Controller(type: typeof(HelpController)));
            return new MicrosoftExtensionsDependencyInjectionBuilder(serviceCollection, recognizes);
        }

        /// <summary>
        /// Create an instance of builder that uses IServiceCollection to configure IServiceProvider
        /// </summary>
        public static IAppHostBuilder Create()=>
            Create(configuration: null, serviceCollection: null);

        /// <summary>
        /// Create an instance of builder using IServiceProvider to get dependencies.
        /// Note that you need to wire up controllers on your own since the builder cannot change the registrations.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static IAppHostBuilder Create(IServiceProvider serviceProvider)
        {
            var recognizes = new RecognizesBuilder();
            recognizes.Recognizes.Add(new Domain.Controller(type: typeof(HelpController)));
            return new ServiceProviderAppHostBuilder(serviceProvider, recognizes);
        }
    }
}
