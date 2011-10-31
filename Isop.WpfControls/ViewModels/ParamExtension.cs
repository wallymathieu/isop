using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Isop.WpfControls.ViewModels
{
    public static class ParamExtension
    {
        public static ParsedArguments GetParsedArguments(this IEnumerable<Param> parms)
        {
            var recognizedArguments = parms
                .Where(p => !String.IsNullOrWhiteSpace(p.Value))
                .Select(p => p.RecognizedArgument()).ToList();
            var argumentWithOptions = parms.Select(p => p.ArgumentWithOptions).ToList();
            return new ParsedArguments
                       {
                           RecognizedArguments = recognizedArguments,
                           ArgumentWithOptions = argumentWithOptions,
                           UnRecognizedArguments = new List<UnrecognizedArgument>().ToList()
                       };
        }

        public static ParsedMethod Parse(this Build argumentParserBuilder, Method currentMethod)
        {
            var controllerRecognizer = argumentParserBuilder.GetControllerRecognizers()
                .First(c => c.ClassName().Equals(currentMethod.ClassName));

            var parsedArguments = currentMethod.GetParsedArguments();
            var unMatchedRequiredArguments = parsedArguments.UnMatchedRequiredArguments();
            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments")
                {
                    Arguments = unMatchedRequiredArguments
                        .Select(unmatched => new KeyValuePair<string, string>(unmatched.Argument.ToString(), unmatched.Argument.Help())).ToList()
                };
            }

            var method = controllerRecognizer.GetMethods()
                .First(m => m.Name.Equals(currentMethod.Name));
            var parsedMethod = controllerRecognizer.Parse(method, parsedArguments);
            parsedMethod.Factory = argumentParserBuilder.GetFactory();
            return parsedMethod;
        }

        public static MethodTreeModel GetMethodTreeModel(this Build argumentParserBuilder)
        {
            return new MethodTreeModel
                       {
                           Controllers = argumentParserBuilder.GetControllerRecognizers()
                               .Where(cmr => !cmr.ClassName().Equals("help", StringComparison.OrdinalIgnoreCase))
                               .Select(cmr => new Controller
                                                  {
                                                      Name = cmr.ClassName(),
                                                      Methods = cmr.GetMethods().Select(m => new Method(m.Name, cmr.ClassName())
                                                        {
                                                            Parameters = new ObservableCollection<Param>(
                                                                m.GetParameters().Select(p =>
                                                                    new Param(p.ParameterType, p.Name, 
                                                                        new ArgumentWithOptions(p.Name,required:true))))
                                                        })
                                                  }),
                           GlobalParameters = new ObservableCollection<Param>(argumentParserBuilder.GetGlobalParameters()
                                                                                  .Select(p => new Param(typeof(string), p.Argument.ToString(), p)))
                       };
        }
    }
}