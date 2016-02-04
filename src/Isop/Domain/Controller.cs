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
        public Controller(Type type, bool ignoreGlobalUnMatchedParameters)
        {
            Type = type;
            IgnoreGlobalUnMatchedParameters = ignoreGlobalUnMatchedParameters;
        }
        public bool IgnoreGlobalUnMatchedParameters { get; private set; }
        public Type Type
        {
            get;
            set;
        }
        public string Name { get { return ControllerName(Type); } }

        private static string ControllerName(Type type)
        {
            return Regex.Replace(type.Name, Conventions.ControllerName + "$", "", RegexOptions.IgnoreCase);
        }

        public IEnumerable<Method> GetControllerActionMethods()
        {
            return GetControllerActionMethods(Type).Select(m => new Method(m) { Controller = this });
        }

        private static IEnumerable<MethodInfo> GetControllerActionMethods(Type type)
        {
            return GetOwnPublicMethods(type)
                .Where(m => !m.Name.EqualsIgnoreCase(Conventions.Help));
        }

        private static IEnumerable<MethodInfo> GetOwnPublicMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.DeclaringType != typeof(Object))
                .Where(m => !m.Name.StartsWithIgnoreCase("get_")
                    && !m.Name.StartsWithIgnoreCase("set_"))
                ;
        }

        public bool Recognize(string controllerName)
        {
            return Name.EqualsIgnoreCase(controllerName);
        }

        public bool Recognize(string controllerName, string actionName)
        {
            return Name.EqualsIgnoreCase(controllerName)
                && GetMethod(actionName) != null;
        }
        
        public Method GetMethod(string name)
        {
            return GetControllerActionMethods().SingleOrDefault(m => m.Name.EqualsIgnoreCase(name));
        }
        
        public bool IsHelp()
        {
            return Type == typeof(HelpController); // Is there a better way to do this? 
        }
        
        public override bool Equals(object obj)
        {
            var controller = obj as Controller;
            if (controller != null)
            {
                return Equals(controller);
            }
            return false;
        }
        public bool Equals(Controller obj)
        {
            return Type == obj.Type;
        }
        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[Controller: Type={0}, Name={1}]", Type, Name);
        }
    }
}

