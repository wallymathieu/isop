using System;
using System.Collections.Generic;
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
        public bool IgnoreGlobalUnMatchedParameters{ get; private set;}
        public Type Type
        {
            get;
            set;
        }
        public string Name{ get{ return ControllerName(Type);}}

        private static string ControllerName(Type type)
        {
            return Regex.Replace(type.Name,Conventions.ControllerName+"$", "", RegexOptions.IgnoreCase);
        }

        public IEnumerable<Method> GetControllerActionMethods()
        {
            return GetControllerActionMethods(Type).Select(m => new Method(m){ Controller = this });
        }

        private static IEnumerable<MethodInfo> GetControllerActionMethods(Type type)
        {
            return GetOwnPublicMethods(type)
                .Where(m => !m.Name.EqualsIC(Conventions.Help));
        }
        private static IEnumerable<MethodInfo> GetOwnPublicMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public| BindingFlags.Instance)
                .Where(m=>!m.DeclaringType.Equals(typeof(Object)))
                .Where(m => !m.Name.StartsWithIC("get_")
                    && !m.Name.StartsWithIC("set_"))
                ;
        }
        public Method GetMethod(string name){
            return GetControllerActionMethods().SingleOrDefault(m => m.Name.EqualsIC(name));
        }
        public bool IsHelp(){
            return Type == typeof(HelpController); // Is there a better way to do this? 
        }
        public override bool Equals(object obj)
        {
            if (obj is Controller)
            {
                return Equals((Controller)obj);
            }
            return false;
        }
        public bool Equals(Controller obj)
        {
            return Type.Equals(obj.Type);
        }
        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("[Controller: Type={0}, Name={1}]", Type, Name);
        }
    }
}

