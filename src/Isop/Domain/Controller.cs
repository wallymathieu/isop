using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;


namespace Isop.Domain
{
    using Infrastructure;
    using Help;

    public class Controller(Type type)
    {
        public Type Type { get; } = type;
        public string GetName(Conventions conventions)
        {
            if (conventions is null) throw new ArgumentNullException(nameof(conventions));
            return Regex.Replace(Type.Name, conventions.ControllerName + "$", "", RegexOptions.IgnoreCase);
        }

        public IEnumerable<Method> GetControllerActionMethods(Conventions conventions) => 
            GetControllerActionMethods(conventions, Type).Select(m => new Method(m));

        private static IEnumerable<MethodInfo> GetControllerActionMethods(Conventions conventions, Type type) =>
            GetOwnPublicMethods(type)
                .Where(m => !m.Name.EqualsIgnoreCase(conventions.Help));

        private static IEnumerable<MethodInfo> GetOwnPublicMethods(Type type) =>
            type.GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.DeclaringType != typeof(object))
                .Where(m => !m.Name.StartsWithIgnoreCase("get_")
                            && !m.Name.StartsWithIgnoreCase("set_"));

        public Method? GetMethod(Conventions conventions, string name) => 
            GetControllerActionMethods(conventions).SingleOrDefault(m => m.Name.EqualsIgnoreCase(name));

        public bool IsHelp() => Type == typeof(HelpController);

        public override bool Equals(object? obj) => obj is Controller controller && Equals(controller);
        public bool Equals(Controller obj) => Type == obj?.Type;

        public override int GetHashCode() => Type.GetHashCode();

        public override string ToString() => $"[Controller: Type={Type}]";
    }
}

