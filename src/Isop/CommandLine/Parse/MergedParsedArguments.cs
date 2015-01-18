using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Isop.CommandLine.Parse
{
    public class MergedParsedArguments : ParsedArguments
    {
        private readonly ParsedArguments _first;
        private readonly ParsedArguments _second;
        public MergedParsedArguments(ParsedArguments first, ParsedArguments second) 
            : base(first.Args)
        {
            _first = first;
            _second = second;
            RecognizedArguments = first.RecognizedArguments.Union(second.RecognizedArguments);
            ArgumentWithOptions = first.ArgumentWithOptions.Union(second.ArgumentWithOptions);
            UnRecognizedArguments = first.UnRecognizedArguments.Intersect(second.UnRecognizedArguments);
        }
        public override IEnumerable<string> Invoke()
        {
            foreach (var item in _first.Invoke())
            {
                yield return item;
            };
            foreach (var item in _second.Invoke())
            {
                yield return item;
            }
        }
    }
}

