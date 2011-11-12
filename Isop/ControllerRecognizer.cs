﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Collections;
using TypeConverterFunc=System.Func<System.Type,string,System.Globalization.CultureInfo,object>;
namespace Isop
{
    
    [Serializable]
    public class TypeConversionFailedException : Exception
    {
        public string Argument;
        public string Value;
        public Type TargetType;
     public TypeConversionFailedException ()
     {
     }
     
     public TypeConversionFailedException (string message) : base (message)
     {
     }
     
     public TypeConversionFailedException (string message, Exception inner) : base (message, inner)
     {
     }
     
    }
	public class Transform
    {
        // Lexer -> 
        // Arg(ControllerName),Param(..),.. -> Arg(ControllerName),Arg('Index'),... 
        public ArgumentLexer Rewrite(ArgumentLexer tokens)
        {
            //"--command"
            if (tokens.Count()>=2 
                && tokens[0].TokenType==TokenType.Argument 
                && tokens[0].Value.Equals("help",StringComparison.OrdinalIgnoreCase)
                && tokens[1].TokenType==TokenType.Argument)
            {
                tokens[1] = new Token(tokens[1].Value,TokenType.ParameterValue,tokens[1].Index);
                tokens.Insert(1,new Token("command",TokenType.Parameter,1));
            }
            //help maps to index (should have routing here)
            if (tokens.Count() == 0)
            {
                tokens.Add(new Token("help",TokenType.Argument,0));
            }

            //Index rewrite:
            var indexToken= new Token("Index", TokenType.Argument,1);
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
    public class ControllerRecognizer
    {
        private static MethodInfo FindMethod (IEnumerable<MethodInfo> methods, String methodName, IEnumerable<Token> lexer)
        {
            var potential = methods
                .Where (method => method.Name.Equals (methodName, StringComparison.OrdinalIgnoreCase));
            var methodInfo = potential
                .Where(method=>method.GetParameters().Length<=lexer.Count(t=>t.TokenType==TokenType.Parameter))
                .OrderByDescending(method=>method.GetParameters().Length)
                .FirstOrDefault();
            if (methodInfo==null)
            {
                methodInfo = potential.FirstOrDefault();
            }
            return methodInfo;
        }

        private static IEnumerable<Object> GetParametersForMethod (MethodInfo method, ParsedArguments parsedArguments, Func<RecognizedArgument,ParameterInfo,Object> convertFrom)
        {
            var parameterInfos = method.GetParameters();
            var parameters = new List<Object>();
            
            foreach (var paramInfo in parameterInfos)
            {
                var recognizedArgument =  parsedArguments.RecognizedArguments.First(
                    a => a.Argument.ToUpperInvariant().Equals(paramInfo.Name.ToUpperInvariant()));
                parameters.Add( convertFrom (recognizedArgument, paramInfo));
            }
            return parameters;
        }

        private IEnumerable<ArgumentWithOptions> GetRecognizers(MethodBase method)
        {
            var parameterInfos = method.GetParameters();
            var recognizers = parameterInfos
                .Select (parameterInfo => 
                    new ArgumentWithOptions (ArgumentParameter.Parse (parameterInfo.Name,_culture), required: true))
                .ToList ();
            recognizers.Insert(0, new ArgumentWithOptions(ArgumentParameter.Parse("#1" + method.Name, _culture), required: false));
            return recognizers;
        }
        
        private readonly CultureInfo _culture;
        public Type Type { get; private set; }
		public bool IgnoreGlobalUnMatchedParameters{get; private set;}
		private readonly Transform _transform = new Transform();
        /// <summary>
        /// </summary>
        public ControllerRecognizer(Type type, 
            CultureInfo cultureInfo = null, 
            TypeConverterFunc typeConverter = null,
            bool ignoreGlobalUnMatchedParameters = false)
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

            var recognizedActionParameters = GetParametersForMethod(methodInfo, parsedArguments, ConvertFrom1);
            
            parsedArguments.UnRecognizedArguments = parsedArguments.UnRecognizedArguments
                .Where(unrecognized=>unrecognized.Index>=1); //NOTE: should be different!
          
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
                throw new TypeConversionFailedException("Could not convert argument", e){
                    Argument=arg1.WithOptions.Argument.ToString(),
                    Value=arg1.Value,
                    TargetType=parameterInfo.ParameterType 
                };
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
		
        public override void Invoke(TextWriter cout)
        {
			var instance = Factory(RecognizedClass);
			
			var retval = RecognizedAction.Invoke(instance, RecognizedActionParameters.ToArray());
            if (retval == null) return;
            if (retval is string)
            {
                cout.Write(retval as string);
            }else if (retval is IEnumerable)
            {
                foreach (var item in (retval as IEnumerable)) {
                    cout.Write(item.ToString());
                }
            }else
            {
                cout.Write(retval.ToString());
            }
        }
    }
}