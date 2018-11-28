using System;
using System.Collections.Generic;
using System.Globalization;
using Isop.Help;
using Isop.Infrastructure;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Isop.Configurations
{
    using Domain;
    internal class ConfigureUsingInstance
    {
        private readonly Configuration _configuration;
        private readonly HelpXmlDocumentation _helpXmlDocumentation;
        private readonly IServiceCollection _serviceCollection;

        public ConfigureUsingInstance(Configuration configuration, HelpXmlDocumentation helpXmlDocumentation, 
            IServiceCollection serviceCollection)
        {
            _configuration = configuration;
            _helpXmlDocumentation = helpXmlDocumentation;
            _serviceCollection = serviceCollection;
        }

        static MethodInfo[] GetPublicInstanceMethods(Type t)
        {
            return t.GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.Public);
        }

        void ConfigureRecognizes(Type t, object instance, MethodInfo[] methods = null)
        {
            var recognizesMethod = MatchGet((methods ?? GetPublicInstanceMethods(t)),
                parameters: new Type[0],
                name: new Regex("Recognizes", RegexOptions.IgnoreCase),
                returnType: typeof(IEnumerable<Type>)
                );
            if (null == recognizesMethod) return; // could not find Recognizes
            
            var recognizes = (IEnumerable<Type>)recognizesMethod.Invoke(instance, new object[0]);
            foreach (var recognized in recognizes)
            {
                _configuration.Recognizes.Add(new Controller(recognized, false));
                _serviceCollection.TryAddSingleton(recognized);
            }
        }

        void ConfigurationPublicWritableFields(object instance, MethodInfo[] methods)
        {
            var configurationSetters = FindSet(methods);
            foreach (var methodInfo in configurationSetters)
            {
                var action = (Action<String>)methodInfo.MethodInfo.CreateDelegate(typeof(Action<String>), instance);
                var description = _helpXmlDocumentation.GetDescriptionForMethod(methodInfo.MethodInfo);
                _configuration.Properties.Add(new Property(RemoveSetFromBeginningOfString(methodInfo.Name), action: action, required: methodInfo.Required, description: description, type: typeof(string)));
            }
        }

        private static readonly Regex _set = new Regex("^set_?", RegexOptions.IgnoreCase);
        private static readonly Regex _get = new Regex("^get_?", RegexOptions.IgnoreCase);
        private static string RemoveSetFromBeginningOfString(string arg)
        {
            return _set.Replace(arg, "");
        }

        void CultureInfo(object instance, MethodInfo[] methods)
        {
            var culture = MatchGet(methods,
                name: new Regex("Culture(Info)?", RegexOptions.IgnoreCase),
                returnType: typeof(CultureInfo),
                parameters: new Type[0]);
            if (null != culture)
            {
                _configuration.CultureInfo = (CultureInfo)culture.Invoke(instance, new object[0]);
            }
        }

        void RecognizeHelp(object instance, MethodInfo[] methods)
        {
            var recongizeHelp = MatchGet(methods, name: new Regex("(Recognize)?Help", RegexOptions.IgnoreCase), returnType: typeof(bool), parameters: new Type[0]);
            if (null != recongizeHelp && (bool)recongizeHelp.Invoke(instance, new object[0]))
            {
                _configuration.RecognizesHelp = true;
            }
        }

        void TypeConverter(object instance, MethodInfo[] methods)
        {
            var typeconv = MatchGet(methods,
                name: new Regex("(Type)?Converter", RegexOptions.IgnoreCase),
                returnType: typeof(object),
                parameters: new Type[] { typeof(Type), typeof(string), typeof(CultureInfo) });
            if (null != typeconv)
            {
                _configuration.TypeConverter = (type, value, culture) => typeconv.Invoke(instance, new object[] { type, value, culture });
            }
            else
            {
                ReturnsFuncTypeConverter(instance, methods);
            }
        }

        private void ReturnsFuncTypeConverter(object instance, MethodInfo[] methods)
        {
            // old style
            var getTypeconv = MatchGet(methods,
                name: new Regex("(Type)?Converter", RegexOptions.IgnoreCase),
                returnType: typeof(Func<Type, string, CultureInfo, object>),
                parameters: new Type[0]);
            if (null != getTypeconv)
            {
                var typeConv = (Func<Type, string, CultureInfo, object>)getTypeconv.Invoke(instance, new object[0]);
                _configuration.TypeConverter = (type, value, culture) => typeConv(type, value, culture);
            }
        }

        public void Configure(Type t, object instance)
        {
            var methods = GetPublicInstanceMethods(t);

            CultureInfo(instance, methods);

            ConfigureRecognizes(t, instance, methods);

            ConfigurationPublicWritableFields(instance, methods);

            RecognizeHelp(instance, methods);

            TypeConverter(instance, methods);
        }

        static IEnumerable<MethodInfoOrProperty> FindSet(IEnumerable<MethodInfo> methods)
        {
            return methods
                .Where(m => _set.IsMatch(m.Name))
                .Select(m => new MethodInfoOrProperty(m));
        }

        static MethodInfoOrProperty MatchGet(IEnumerable<MethodInfo> methods, Regex name, Type returnType, IEnumerable<Type> parameters)
        {
            return methods.Where(m =>
                m.ReturnType.Equals(returnType)
                && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameters)
                && name.IsMatch(_get.Replace(m.Name, "")))
                .Select(m => new MethodInfoOrProperty(m))
                .FirstOrDefault();
        }
    }
}