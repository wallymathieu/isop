using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Helpers.Console
{
	public class Transform
    {
        public MethodInfo Accept (IEnumerable<MethodInfo> methods, String methodName)
        {
            var methodInfo = methods.FirstOrDefault (method => method.Name.Equals (methodName, StringComparison.OrdinalIgnoreCase)) ??
                                 methods.FirstOrDefault (method => method.Name.Equals ("index", StringComparison.OrdinalIgnoreCase));
            return methodInfo;
        }

        public IEnumerable<Object> GetParametersForMethod (MethodInfo method, IEnumerable<string> arg, ParsedArguments parsedArguments, Func<RecognizedArgument,ParameterInfo,Object> ConvertFrom1)
        {
            var parameterInfos = method.GetParameters ();
            var parameters = (from paramInfo in parameterInfos
                   join recognizedArgument in parsedArguments.RecognizedArguments on
                                                paramInfo.Name.ToLowerInvariant ()
                                                equals recognizedArgument.Argument.ToLowerInvariant ()
                   select ConvertFrom1 (recognizedArgument, paramInfo)).ToList ();
            return parameters;
        }

        public IEnumerable<ArgumentWithOptions> GetRecognizers (MethodInfo method)
        {
            var parameterInfos = method.GetParameters ();
            var recognizers = parameterInfos
                .Select (parameterInfo => 
                    new ArgumentWithOptions (ArgumentParameter.Parse (parameterInfo.Name), required: true))
                .ToList ();
            recognizers.Insert(0,new ArgumentWithOptions(ArgumentParameter.Parse("#1"+method.Name), required: true));
            return recognizers;
        }
    }
	
    public delegate object TypeConverterFunc(Type type, string s, CultureInfo cultureInfo);
    public class ClassAndMethodRecognizer
    {
        private readonly CultureInfo _culture;
        public Type Type { get; private set; }
		private readonly object instance;
		private readonly Transform transform = new Transform();
        /// <summary>
        /// </summary>
        public ClassAndMethodRecognizer(Object instance=null,Type type=null, CultureInfo cultureInfo = null, TypeConverterFunc typeConverter = null)
        {
			this.instance = instance;
            _typeConverter = typeConverter ?? DefaultConvertFrom;
            Type = type;
            _culture = cultureInfo ?? CultureInfo.CurrentCulture;
        }
		
        public bool Recognize(IEnumerable<string> arg)
        {
            return null != FindMethodInfo(arg);
        }

        private MethodInfo FindMethodInfo(IEnumerable<string> arg)
        {
            var foundClassName = ClassName().Equals(arg.ElementAtOrDefault(0), StringComparison.OrdinalIgnoreCase);
            if (foundClassName)
            {
                var methodName = arg.ElementAtOrDefault(1);
				var methodInfo = transform.Accept(GetMethods(),methodName);
                return methodInfo;
            }
            return null;
        }
		private IEnumerable<MethodInfo> GetMethods()
		{
			return Type.GetMethods(BindingFlags.Public| BindingFlags.Instance).Where(m=>!m.DeclaringType.Equals(typeof(Object)));
		}
		
        public string ClassName()
        {
            return Type.Name.Replace("Controller", "");
        }

        /// <summary>
        /// Note that in order to register a converter you can use:
        /// TypeDescriptor.AddAttributes(typeof(AType), new TypeConverterAttribute(typeof(ATypeConverter)));
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public ParsedMethod Parse(IEnumerable<string> arg)
        {
            var methodInfo = FindMethodInfo(arg);

            var argumentRecognizers = transform.GetRecognizers(methodInfo);

            var parser = new ArgumentParser(argumentRecognizers);

            var parsedArguments = parser.Parse(arg);
            var recognizedActionParameters = transform.GetParametersForMethod(methodInfo, arg, parsedArguments, ConvertFrom1);
            
			parsedArguments.UnRecognizedArguments = parsedArguments.UnRecognizedArguments
				.Where(unrecognized=>unrecognized.Index>=2); //NOTE: should be different!
			
            return new ParsedMethod(parsedArguments)
                       {
                           RecognizedAction = methodInfo,
                           RecognizedActionParameters = recognizedActionParameters,
                           RecognizedClass = Type,
						   Instance = instance
                       };
        }

        private object ConvertFrom1(RecognizedArgument arg1, ParameterInfo parameterInfo)
        {
            try
            {
                return _typeConverter(parameterInfo.ParameterType, arg1.Value, _culture);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Could not parse {0} with value: {1}", arg1.WithOptions.Argument, arg1.Value), e);
            }
        }
        private readonly TypeConverterFunc _typeConverter;
        private static object DefaultConvertFrom(Type type, string s, CultureInfo cultureInfo)
        {
            return TypeDescriptor.GetConverter(type).ConvertFrom(null, cultureInfo, s);
        }

        public string Help(bool simpleDescription)
        {
            if (simpleDescription) return ClassName();
			
			return ClassName()
				+Environment.NewLine
				+Environment.NewLine
				+String.Join(Environment.NewLine, GetMethods().Select(m=>"  "+m.Name).ToArray());
        }
    }

    public class ParsedMethod : ParsedArguments
    {
        public ParsedMethod(ParsedArguments parsedArguments)
            : base(parsedArguments)
        {
        }
		public Func<Type,Object> Factory{get;set;}
		
        public Type RecognizedClass;
        public MethodInfo RecognizedAction { get; set; }

        public IEnumerable<object> RecognizedActionParameters { get; set; }
		public Object Instance { get; set; }
        public override String Invoke()
        {
			object instance;
			if (null==Instance)
			{
				var factory = this.Factory ??Activator.CreateInstance;
				instance = factory(RecognizedClass);
			}else
			{
				instance = Instance;
			}
			var retval = RecognizedAction.Invoke(instance, RecognizedActionParameters.ToArray());
			if (retval != null)
			{
				return retval.ToString();
			}else
			{
				return "";
			}
		}
    }
}