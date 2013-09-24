using System.IO;
using System.Linq;

namespace Isop.Parse
{
    public class MergedParsedArguments : ParsedArguments
    {
        private readonly ParsedArguments _first;
        private readonly ParsedArguments _second;
        public MergedParsedArguments(ParsedArguments first, ParsedArguments second)
        {
            _first = first;
            _second = second;
            RecognizedArguments = first.RecognizedArguments.Union(second.RecognizedArguments);
            ArgumentWithOptions = first.ArgumentWithOptions.Union(second.ArgumentWithOptions);
            UnRecognizedArguments = first.UnRecognizedArguments.Intersect(second.UnRecognizedArguments);
        }
        public override void Invoke(TextWriter cout)
        {
            _first.Invoke(cout);
            _second.Invoke(cout);
        }
    }
}

