using System.Collections.Generic;
using System.Linq;

namespace Isop.Gui
{
    public static class ParamExtension
    {
        public static ParsedArguments GetParsedArguments(this IEnumerable<Param> parms)
        {
            var recognizedArguments = parms
                .Select(p => p.RecognizedArgument()).ToList();
            var argumentWithOptions = parms.Select(p => p.ArgumentWithOptions).ToList();
            return new ParsedArguments
                       {
                           RecognizedArguments = recognizedArguments,
                           ArgumentWithOptions = argumentWithOptions,
                           UnRecognizedArguments = new List<UnrecognizedArgument>().ToList()
                       };
        }
    }
}