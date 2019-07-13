using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace Isop
{
    using Infrastructure;
    using Help;
    using CommandLine;
    using CommandLine.Lex;
    using CommandLine.Parse;
    using CommandLine.Help;
    using Domain;
    using Api;
    using Microsoft.Extensions.Options;

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
            serviceCollection.TryAddSingleton(new Formatter(new ToStringFormatter().Format));
            serviceCollection.TryAddSingleton(new TypeConverterFunc(new DefaultConverter().ConvertFrom));
            if (configuration != null)
            {
                serviceCollection.AddSingleton(Options.Create(configuration));
            }

            return new Builder(serviceCollection);
        }
    }
}
