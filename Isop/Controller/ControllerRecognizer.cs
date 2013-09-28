using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Isop.Infrastructure;
using Isop.Lex;
using Isop.Parse;

namespace Isop.Controller
{
    public class ControllerRecognizer
    {
        private static MethodInfo FindMethod(IEnumerable<MethodInfo> methods, String methodName, IEnumerable<Token> lexed)
        {
            var potential = methods
                .Where(method => method.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase));
            var potentialMethod = potential
                .Where(method => method.GetParameters().Length <= lexed.Count(t => t.TokenType == TokenType.Parameter))
                .OrderByDescending(method => method.GetParameters().Length)
                .FirstOrDefault();
            if (potentialMethod != null)
            {
                return potentialMethod;
            }
            return potential.FirstOrDefault();
        }


        public IEnumerable<ArgumentWithOptions> GetMethodParameterRecognizers(MethodInfo methodInfo)
        {
            return _turnParametersToArgumentWithOptions.GetRecognizers(methodInfo);
        }

        public IEnumerable<ArgumentWithOptions> GetRecognizers(string methodname)
        {//For tests mostly
            return _turnParametersToArgumentWithOptions.GetRecognizers(Type.GetMethod(methodname));
        }

        private readonly CultureInfo _culture;
        public Type Type { get; private set; }
        public bool IgnoreGlobalUnMatchedParameters { get; private set; }
        private readonly RewriteLexedTokensToSupportHelpAndIndex _rewriteLexedTokensToSupportHelpAndIndex = new RewriteLexedTokensToSupportHelpAndIndex();
        /// <summary>
        /// </summary>
        public ControllerRecognizer(Type type, CultureInfo cultureInfo=null, Func<Type, string, CultureInfo, object> typeConverter = null, bool ignoreGlobalUnMatchedParameters = false, bool allowInferParameter=false)
        {
            Type = type;
            _culture = cultureInfo ?? CultureInfo.CurrentCulture;
            IgnoreGlobalUnMatchedParameters = ignoreGlobalUnMatchedParameters;
            _allowInferParameter = allowInferParameter;
            _turnParametersToArgumentWithOptions = new TurnParametersToArgumentWithOptions(_culture, typeConverter);
        }

        public bool Recognize(IEnumerable<string> arg)
        {
            var lexed = _rewriteLexedTokensToSupportHelpAndIndex.Rewrite(ArgumentLexer.Lex(arg).ToList());
            return null != FindMethodInfo(lexed);
        }

        private MethodInfo FindMethodInfo(IEnumerable<Token> arg)
        {
            var foundClassName = ClassName().Equals(arg.ElementAtOrDefault(0).Value, StringComparison.OrdinalIgnoreCase);
            if (foundClassName)
            {
                var methodName = arg.ElementAtOrDefault(1).Value;
                var methodInfo = FindMethod(GetMethods(), methodName, arg);
                return methodInfo;
            }
            return null;
        }
        public IEnumerable<MethodInfo> GetMethods()
        {
            return ReflectionExtensions.GetOwnPublicMethods(Type)
                .Where(m => !m.Name.Equals("help", StringComparison.OrdinalIgnoreCase));
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
            var lexed = _rewriteLexedTokensToSupportHelpAndIndex.Rewrite(ArgumentLexer.Lex(arg).ToList());

            var methodInfo = FindMethodInfo(lexed);

            var argumentRecognizers = _turnParametersToArgumentWithOptions.GetRecognizers(methodInfo)
                .ToList();
            argumentRecognizers.InsertRange(0, new[] { 
                new ArgumentWithOptions(ArgumentParameter.Parse("#0" + ClassName(), _culture), required: true, type: typeof(string)),
                new ArgumentWithOptions(ArgumentParameter.Parse("#1" + methodInfo.Name, _culture), required: false, type: typeof(string))
            });

            var parser = new ArgumentParser(argumentRecognizers, _allowInferParameter);
            var parsedArguments = parser.Parse(lexed, arg);

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

            var recognizedActionParameters = _turnParametersToArgumentWithOptions.GetParametersForMethod(methodInfo,
                            parsedArguments);

            return new ParsedMethod(parsedArguments)
                       {
                           RecognizedAction = methodInfo,
                           RecognizedActionParameters = recognizedActionParameters,
                           RecognizedClass = Type
                       };
        }

        private bool _allowInferParameter;
        private TurnParametersToArgumentWithOptions _turnParametersToArgumentWithOptions;

        private MethodInfo GetMethod(string action)
        {
            return GetMethods().SingleOrDefault(m => m.WithName(action));
        }

        public class MethodHelp
        {
            public MethodHelp(MethodInfo methodinfo, ControllerRecognizer cr)
            {
                this.Method = methodinfo;
                this._cr = cr;
            }
            private readonly ControllerRecognizer _cr;
            public string Name { get { return Method.Name; } }

            public MethodInfo Method { get; private set; }
            public IEnumerable<ArgumentWithOptions> MethodParameters()
            {
                return _cr.GetRecognizers(Method);
            }
        }

        public MethodHelp GetMethodHelp(string action)
        {
            return new MethodHelp(GetMethod(action),this);
        }

        internal IEnumerable<ArgumentWithOptions> GetRecognizers(MethodInfo method)
        {
            return _turnParametersToArgumentWithOptions.GetRecognizers(method);
        }
    }
}