using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Isop.Infrastructure;
using Isop.Parse;

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
            return new ParsedArguments((IList<string>) null)
                       {
                           RecognizedArguments = recognizedArguments,
                           ArgumentWithOptions = argumentWithOptions,
                           UnRecognizedArguments = new List<UnrecognizedArgument>().ToList()
                       };
        }

        public static void SetParamValueOnMatching(this IEnumerable<Param> that, Param updatedParam)
        {
            foreach (var param in that.Where(p => p.Name.EqualsIC(updatedParam.Name)))
            {
                param.Value = updatedParam.Value;
            }
        }

        public static void DeRegisterPropertyChanged(this IEnumerable<Param> that, PropertyChangedEventHandler handler)
        {
            foreach (var param in that)
            {
                param.PropertyChanged -= handler;
            }
        }
        public static void RegisterPropertyChanged(this IEnumerable<Param> that, PropertyChangedEventHandler handler)
        {
            foreach (var param in that)
            {
                param.PropertyChanged += handler;
            }
        }
    }
}