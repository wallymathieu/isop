using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Isop
{
    public class ParsedArguments
    {
        public ParsedArguments()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parsedArguments"></param>
        public ParsedArguments(ParsedArguments parsedArguments)
        {
            RecognizedArguments = parsedArguments.RecognizedArguments;
            ArgumentWithOptions = parsedArguments.ArgumentWithOptions;
            UnRecognizedArguments = parsedArguments.UnRecognizedArguments;
        }
        public IEnumerable<RecognizedArgument> RecognizedArguments { get; set; }

        public IEnumerable<UnrecognizedArgument> UnRecognizedArguments { get; set; }

        public IEnumerable<ArgumentWithOptions> ArgumentWithOptions { get; set; }

        public virtual void Invoke(TextWriter cout)
        {
            foreach (var argument in RecognizedArguments.Where(argument => null != argument.WithOptions.Action))
            {
                argument.WithOptions.Action(argument.Value);
            }
        }

        public ParsedArguments Merge(ParsedArguments args)
        {
            return Merge(this, args);
        }
        public static ParsedArguments Merge(ParsedArguments first, ParsedArguments second)
        {
            return new MergedParsedArguments(first, second);
        }

        public IEnumerable<ArgumentWithOptions> UnMatchedRequiredArguments()
        {
            var unMatchedRequiredArguments = ArgumentWithOptions
                .Where(argumentWithOptions => argumentWithOptions.Required)
                .Where(argumentWithOptions => !RecognizedArguments
                                                   .Any(recogn => recogn.WithOptions.Equals(argumentWithOptions)));
            return unMatchedRequiredArguments;
        }
    }
}

