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

    public class Controller
    {
        public Controller(Type type)
        {
            Type = type;
        }
        public bool IgnoreGlobalUnMatchedParameters { get; }
        public Type Type { get; }
        public string GetName(Conventions conventions) => ControllerName(conventions,Type);

        private string ControllerName(Conventions conventions, Type type)
        {
            return Regex.Replace(type.Name, conventions.ControllerName + "$", "", RegexOptions.IgnoreCase);
        }

        public IEnumerable<Method> GetControllerActionMethods(Conventions conventions)
        {
            return GetControllerActionMethods(conventions, Type).Select(m => new Method(m) { Controller = this });
        }

        private static IEnumerable<MethodInfo> GetControllerActionMethods(Conventions conventions, Type type)
        {
            return GetOwnPublicMethods(type)
                .Where(m => !m.Name.EqualsIgnoreCase(conventions.Help));
        }

        private static IEnumerable<MethodInfo> GetOwnPublicMethods(Type type)
        {
            return type.GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.DeclaringType != typeof(Object))
                .Where(m => !m.Name.StartsWithIgnoreCase("get_")
                    && !m.Name.StartsWithIgnoreCase("set_"))
                ;
        }

        public bool Recognize(Conventions conventions, string controllerName)
        {
            return GetName(conventions).EqualsIgnoreCase(controllerName);
        }

        public bool Recognize(Conventions conventions, string controllerName, string actionName)
        {
            return GetName(conventions).EqualsIgnoreCase(controllerName)
                && GetMethod(conventions, actionName) != null;
        }
        
        public Method GetMethod(Conventions conventions, string name)
        {
            return GetControllerActionMethods(conventions).SingleOrDefault(m => m.Name.EqualsIgnoreCase(name));
        }
        
        public bool IsHelp()
        {
            return Type == typeof(HelpController); // Is there a better way to do this? 
        }
        
        public override bool Equals(object obj)
        {
            if (obj is Controller controller)
            {
                return Equals(controller);
            }
            return false;
        }
        public bool Equals(Controller obj) => Type == obj?.Type;

        public override int GetHashCode() => Type.GetHashCode();

        public override string ToString() => $"[Controller: Type={Type}]";
    }
}

