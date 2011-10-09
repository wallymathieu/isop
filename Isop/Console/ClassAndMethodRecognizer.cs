using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Isop.Console
{
	public class Transform
    {
        // Lexer -> 
        // Arg(ControllerName),Param(..),.. -> Arg(ControllerName),Arg('Index'),... 
        public ArgumentLexer Rewrite(ArgumentLexer arg)
        {
            var indexToken= new Token("Index", TokenType.Argument,1);
            var tokens = arg.ToList();
            if (tokens.Count()>=2 
                && tokens[1].TokenType!=TokenType.Argument 
                && tokens[0].TokenType==TokenType.Argument)
            {
                tokens.Insert(1,indexToken);
            }
            else if (tokens.Count()==1 
                && tokens[0].TokenType==TokenType.Argument)
            {
                tokens.Add(indexToken);
            }
            return new ArgumentLexer(tokens);
        }
    }
	
    public delegate object TypeConverterFunc(Type type, string s, CultureInfo cultureInfo);
    public class ClassAndMethodRecognizer
    {
        private MethodInfo FindMethod (IEnumerable<MethodInfo> methods, String methodName, ArgumentLexer lexer)
        {
            var methodInfo = methods
                .Where (method => method.Name.Equals (methodName, StringComparison.OrdinalIgnoreCase))
                .Where(method=>method.GetParameters().Length<=lexer.Count(t=>t.TokenType==TokenType.Parameter))
                .OrderByDescending(method=>method.GetParameters().Length)
                .FirstOrDefault();
            return methodInfo;
        }

        private IEnumerable<Object> GetParametersForMethod (MethodInfo method, ArgumentLexer arg, ParsedArguments parsedArguments, Func<RecognizedArgument,ParameterInfo,Object> ConvertFrom1)
        {
            var parameterInfos = method.GetParameters();
            var parameters = new List<Object>();
            
            foreach (var paramInfo in parameterInfos) {
                var recognizedArgument =  parsedArguments.RecognizedArguments.First(
                    a=>a.Argument.ToLowerInvariant().Equals(paramInfo.Name.ToLowerInvariant()));
                parameters.Add( ConvertFrom1 (recognizedArgument, paramInfo));
            }
            return parameters;
        }

        private IEnumerable<ArgumentWithOptions> GetRecognizers(MethodInfo method)
        {
            var parameterInfos = method.GetParameters();
            var recognizers = parameterInfos
                .Select (parameterInfo => 
                    new ArgumentWithOptions (ArgumentParameter.Parse (parameterInfo.Name), required: true))
                .ToList ();
            recognizers.Insert(0,new ArgumentWithOptions(ArgumentParameter.Parse("#1"+method.Name), required: false));
            return recognizers;
        }
        
        private readonly CultureInfo _culture;
        public Type Type { get; private set; }
		
		private readonly Transform transform = new Transform();
        /// <summary>
        /// </summary>
        public ClassAndMethodRecognizer(Type type, CultureInfo cultureInfo = null, TypeConverterFunc typeConverter = null)
        {
            _typeConverter = typeConverter ?? DefaultConvertFrom;
            Type = type;
            _culture = cultureInfo ?? CultureInfo.CurrentCulture;
        }
		
        public bool Recognize(IEnumerable<string> arg)
        {
            //TODO: Inefficient
            var lexer = transform.Rewrite( new ArgumentLexer(arg));
            return null != FindMethodInfo(lexer);
        }

        private MethodInfo FindMethodInfo(ArgumentLexer arg)
        {
            var foundClassName = ClassName().Equals(arg.ElementAtOrDefault(0).Value, StringComparison.OrdinalIgnoreCase);
            if (foundClassName)
            {
                var methodName = arg.ElementAtOrDefault(1).Value;
				var methodInfo = FindMethod(GetMethods(),methodName, arg);
                return methodInfo;
            }
            return null;
        }
		public IEnumerable<MethodInfo> GetMethods()
		{
			return Type.GetMethods(BindingFlags.Public| BindingFlags.Instance)
                .Where(m=>!m.DeclaringType.Equals(typeof(Object)))
                .Where(m=>!m.Name.Equals("help",StringComparison.OrdinalIgnoreCase));
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
            var lexer = transform.Rewrite( new ArgumentLexer(arg));
               
            var methodInfo = FindMethodInfo(lexer);

            var argumentRecognizers = GetRecognizers(methodInfo);

            var parser = new ArgumentParser(argumentRecognizers);
            var parsedArguments = parser.Parse(lexer, arg);
            var recognizedActionParameters = GetParametersForMethod(methodInfo, lexer, parsedArguments, ConvertFrom1);
            
			parsedArguments.UnRecognizedArguments = parsedArguments.UnRecognizedArguments
				.Where(unrecognized=>unrecognized.Index>=2); //NOTE: should be different!
			
            return new ParsedMethod(parsedArguments)
                       {
                           RecognizedAction = methodInfo,
                           RecognizedActionParameters = recognizedActionParameters,
                           RecognizedClass = Type
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
		
        public override String Invoke()
        {
			object instance = Factory(RecognizedClass);
			
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