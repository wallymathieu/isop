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

        public static ParsedMethod Parse(this Build that, Method currentMethod)
        {
            var controllerRecognizer = that.ControllerRecognizers
                .First(c => c.ClassName().Equals(currentMethod.ClassName));

            var parsedArguments = currentMethod.Parameters.GetParsedArguments();
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
            parsedMethod.Factory = that.GetFactory();
            return parsedMethod;
        }

        public static MethodTreeModel GetMethodTreeModel(this Build that)
        {
            return new MethodTreeModel(new ObservableCollection<Param>(
                that.GlobalParameters
                    .Select(p => new Param(typeof(string), p.Argument.ToString(), p))),
                that.ControllerRecognizers
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
                        })
            );
        }

        public static void SetParamValueOnMatching(this IEnumerable<Param> that, Param updatedParam)
        {
            foreach (var param in that.Where(p => p.Name.Equals(updatedParam.Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                param.Value = updatedParam.Value;
            }
        }
    }
}