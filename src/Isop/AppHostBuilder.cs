using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Isop
{
    using Abstractions;
    using Domain;
    using Help;
    using Implementations;
    /// <summary>
    /// Utility methods to create builders
    /// </summary>
    public static class AppHostBuilder 
    {
        /// <summary>
        /// Create an instance of builder using IServiceCollection to wire up dependencies
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IAppHostBuilder Create(Configuration configuration) => 
            Create(configuration: configuration, serviceCollection: null);

        /// <summary>
        /// Create an instance of builder that uses IServiceCollection to configure IServiceProvider
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IAppHostBuilder Create(IServiceCollection serviceCollection, Configuration configuration=null)
        {
            if (serviceCollection == null)
            {
                serviceCollection = new ServiceCollection();
            }
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
