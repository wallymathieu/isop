using System;
using System.Reflection;
using Isop.Infrastructure;
using System.Linq;
using System.Globalization;

namespace Isop
{
    public static class BuildExtensions
    {
        public static Build Recognize<T>(this Build build, bool ignoreGlobalUnMatchedParameters = false)
        {
            return build.Recognize(typeof(T), ignoreGlobalUnMatchedParameters);
        }

        public static Build Configuration<T>(this Build build, T instance)
        {
            return build.Configuration(typeof(T), instance);
        }

        public static Build Configuration(this Build build, Type type)
        {
            return build.Configuration(type, Activator.CreateInstance(type));
        }

        public static Build ConfigurationFrom(this Build build, Assembly assembly)
        {
            var autoConfiguration = new AssemblyScanner(assembly);
            var isopconfigurations = autoConfiguration.IsopConfigurations()
                .ToArray();
            foreach (var config in isopconfigurations)
            {
                build.Configuration(config);
            }

            if (!isopconfigurations.Any())
            {
                foreach (var item in autoConfiguration.LooksLikeControllers())
                {
                    build.Recognize(item);
                }
            }
            return build;
        }

        public static Build ConfigurationFromAssemblyPath(this Build build)
        {
            return build.ConfigurationFrom(ExecutionAssembly.Path());
        }

        /// <summary>
        /// Will load all the assemblies in the path in order to scan them.
        /// </summary>
        public static Build ConfigurationFrom(this Build build, string path)
        {
            var loadAssemblies = new LoadAssemblies();
            var assemblies = loadAssemblies.LoadFrom(path);
            foreach (var assembly in assemblies)
            {
                build.ConfigurationFrom(assembly);
            }
            return build;
        }

        /// <summary>
        /// Sets the cultureinfo for the following calls.
        /// </summary>
        /// <param name="build"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static Build SetCulture(this Build build, CultureInfo cultureInfo)
        {
            build.CultureInfo = cultureInfo; return build;
        }

        public static Build SetTypeConverter(this Build build, TypeConverterFunc typeconverter)
        {
            build.TypeConverter = typeconverter; return build;
        }

    }
}

