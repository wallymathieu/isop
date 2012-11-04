using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.IO;
using TypeConverterFunc=System.Func<System.Type,string,System.Globalization.CultureInfo,object>;
namespace Isop
{
    public class ControllerRecognizer
    {
        private static MethodInfo FindMethod (IEnumerable<MethodInfo> methods, String methodName, IEnumerable<Token> lexer)
        {
            var potential = methods
                .Where (method => method.Name.Equals (methodName, StringComparison.OrdinalIgnoreCase));
            var potentialMethod = potential
                .Where(method=>method.GetParameters().Length<=lexer.Count(t=>t.TokenType==TokenType.Parameter))
                .OrderByDescending(method=>method.GetParameters().Length)
                .FirstOrDefault();
            if (potentialMethod!=null)
            {
                return potentialMethod;
            }
            return potential.FirstOrDefault();
        }
        private static bool IsClass(Type t){
            return t.IsClass && t!=typeof(String);
        }
        
        private IEnumerable<Object> GetParametersForMethod (MethodInfo method, 
                      ParsedArguments parsedArguments)
        {//NOTE: Obviously to complicated. Need to refactor.
            var parameterInfos = method.GetParameters();
            var parameters = new List<Object>();
            
            foreach (var paramInfo in parameterInfos)
            {
                if (IsClass(paramInfo.ParameterType) && !IsFile(paramInfo.ParameterType))
                {
                    var obj = Activator.CreateInstance(paramInfo.ParameterType);
                    foreach (
                        PropertyInfo prop in
                            paramInfo.ParameterType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        var recognizedArgument = parsedArguments.RecognizedArguments
                            .First(a => a.Argument.ToUpperInvariant()
                                            .Equals(prop.Name.ToUpperInvariant()));

                        prop.SetValue(obj, ConvertFrom(recognizedArgument, prop.PropertyType), null);
                    }
                    parameters.Add(obj);
                }
                else
                {
                    var recognizedArgument = parsedArguments.RecognizedArguments.First(
                        a => a.Argument.ToUpperInvariant().Equals(paramInfo.Name.ToUpperInvariant()));
                    parameters.Add(ConvertFrom(recognizedArgument, paramInfo.ParameterType));
                }
            }
            return parameters;
        }

        public IEnumerable<ArgumentWithOptions> GetRecognizers(MethodBase method)
        {//NOTE: Obviously to complicated. Need to refactor.
            var parameterInfos = method.GetParameters();
            var recognizers = new List<ArgumentWithOptions>();
            foreach (var parameterInfo in parameterInfos)
            {
                if (IsClass(parameterInfo.ParameterType) && !IsFile(parameterInfo.ParameterType))
                {
                    foreach (
                        var prop in
                            parameterInfo.ParameterType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        var arg = new ArgumentWithOptions(ArgumentParameter.Parse(prop.Name, _culture),
                                                          required: true,
                                                          type: prop.PropertyType);
                        recognizers.Add(arg);
                    }
                }
                else
                {
                    var arg = new ArgumentWithOptions(ArgumentParameter.Parse(parameterInfo.Name, _culture),
                                                      required: true,
                                                      type: parameterInfo.ParameterType);
                    recognizers.Add(arg);
                }
            }
            recognizers.Insert(0, new ArgumentWithOptions(ArgumentParameter.Parse("#1" + method.Name, _culture), required: false, type:typeof(string)));
            return recognizers;
        }

        private static bool IsFile(Type parameterType)
        {
            return parameterType == typeof(FileStream);
        }

        private readonly CultureInfo _culture;
        public Type Type { get; private set; }
		public bool IgnoreGlobalUnMatchedParameters{get; private set;}
		private readonly Transform _transform = new Transform();
        /// <summary>
        /// </summary>
        public ControllerRecognizer(Type type, CultureInfo cultureInfo = null, TypeConverterFunc typeConverter = null, bool ignoreGlobalUnMatchedParameters = false)
        {
            _typeConverter = typeConverter ?? DefaultConvertFrom;
            Type = type;
            _culture = cultureInfo ?? CultureInfo.CurrentCulture;
            IgnoreGlobalUnMatchedParameters = ignoreGlobalUnMatchedParameters;
        } 
		
        public bool Recognize(IEnumerable<string> arg)
        {
            //TODO: Inefficient
            var lexer = _transform.Rewrite( new ArgumentLexer(arg));
            return null != FindMethodInfo(lexer);
        }

        private MethodInfo FindMethodInfo(IEnumerable<Token> arg)
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
			return new MethodInfoFinder().GetOwnPublicMethods(Type)
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
            var lexer = _transform.Rewrite( new ArgumentLexer(arg));
               
            var methodInfo = FindMethodInfo(lexer);

            var argumentRecognizers = GetRecognizers(methodInfo);

            var parser = new ArgumentParser(argumentRecognizers);
            var parsedArguments = parser.Parse(lexer, arg);
            
            return Parse(methodInfo, parsedArguments);
        }

        public ParsedMethod Parse(MethodInfo methodInfo, ParsedArguments parsedArguments)
        {
            var unMatchedRequiredArguments = parsedArguments.UnMatchedRequiredArguments();
            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments")
                          {
                              Arguments = unMatchedRequiredArguments
                                  .Select(unmatched => new KeyValuePair<string, string>(unmatched.Argument.ToString(), unmatched.Argument.Help())).ToList()
                          };
            }

            var recognizedActionParameters = GetParametersForMethod(methodInfo, 
                            parsedArguments);
            
            parsedArguments.UnRecognizedArguments = parsedArguments.UnRecognizedArguments
                .Where(unrecognized=>unrecognized.Index>=1); //NOTE: should be different!
          
            return new ParsedMethod(parsedArguments)
                       {
                           RecognizedAction = methodInfo,
                           RecognizedActionParameters = recognizedActionParameters,
                           RecognizedClass = Type
                       };
        }

        private object ConvertFrom(RecognizedArgument arg1, Type type)
        {
            try
            {
                return _typeConverter(type, arg1.Value, _culture);
            }
            catch (Exception e)
            {
                throw new TypeConversionFailedException("Could not convert argument", e){
                    Argument=arg1.WithOptions.Argument.ToString(),
                    Value=arg1.Value,
                    TargetType=type 
                };
            }
        }
     
        private readonly TypeConverterFunc _typeConverter;
        private static object DefaultConvertFrom(Type type, string s, CultureInfo cultureInfo)
        {
            if (type == typeof(FileStream))
            {
                return new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            return TypeDescriptor.GetConverter(type).ConvertFrom(null, cultureInfo, s);
        }
    }

}