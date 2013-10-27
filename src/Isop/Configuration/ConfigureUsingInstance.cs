using System;
using System.Collections.Generic;
using System.Globalization;
using Isop.Help;
using Isop.Infrastructure;
using Isop.Parse;

namespace Isop.Configuration
{
    public abstract class ConfigureUsingInstance
    {
        public virtual CultureInfo CultureInfo { get; set; }
        public abstract ICollection<Type> Recognizes { get; }
        public abstract HelpXmlDocumentation HelpXmlDocumentation { get; }

        public abstract Func<Type, object> Factory { get; set; }

        public abstract Func<Type, string, CultureInfo, object> TypeConverter { get; set; }

        public abstract bool RecognizeHelp { get; set; }
        public abstract IList<ArgumentWithOptions> ArgumentRecognizers { get; }

        public void Configure(Type t, object instance)
        {
            var methods = t.GetPublicInstanceMethods();

            var culture = methods.MatchGet(name: "Culture",
                                           returnType: typeof(CultureInfo),
                                           parameters: new Type[0]);
            if (null != culture)
                CultureInfo = (CultureInfo)culture.Invoke(instance, new object[0]);

            var recognizesMethod = methods.MatchGet(name: "Recognizes",
                                                    returnType: typeof(IEnumerable<Type>),
                                                    parameters: new Type[0]);
            if (null != recognizesMethod)
            {
                var recognizes = (IEnumerable<Type>)recognizesMethod.Invoke(instance, new object[0]);
                foreach (var recognized in recognizes)
                {
                    Recognizes.Add(recognized);
                }
            }
            var objectFactory = methods.Match(name: "ObjectFactory",
                                              returnType: typeof(object),
                                              parameters: new[] { typeof(Type) });
            if (null != objectFactory)
                Factory = (Func<Type, object>)Delegate.CreateDelegate(typeof(Func<Type, object>), instance, objectFactory);

            var configurationSetters = methods.FindSet();
            foreach (var methodInfo in configurationSetters)
            {
                var action = (Action<String>)Delegate.CreateDelegate(typeof(Action<String>),
                                                                      instance, methodInfo.MethodInfo);
                var description = HelpXmlDocumentation.GetDescriptionForMethod(methodInfo.MethodInfo);
                ArgumentRecognizers.Add(new ArgumentWithOptions(methodInfo.Name.RemoveSetFromBeginningOfString(),
                                                                      action: action,
                                                                      required: methodInfo.Required,
                                                                      description: description,
                                                                      type: typeof(string)));

            }
            var recongizeHelp = methods.MatchGet(name: "RecognizeHelp",
                                                 returnType: typeof(bool),
                                                 parameters: new Type[0]);

            if (null != recongizeHelp && (bool)recongizeHelp.Invoke(instance, new object[0]))
            {
                RecognizeHelp = true;
            }

            var typeconv = methods.MatchGet(name: "TypeConverter",
                                            returnType: typeof(Func<Type, string, CultureInfo, object>),
                                            parameters: new Type[0]);
            if (null != typeconv)
            {
                TypeConverter = (Func<Type, string, CultureInfo, object>)typeconv.Invoke(instance, new object[0]);
            }
        }
    }
}