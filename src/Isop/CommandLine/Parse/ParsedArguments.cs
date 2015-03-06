using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isop.Domain;
using System;
namespace Isop.CommandLine.Parse
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

        public IEnumerable<Argument> ArgumentWithOptions { get; set; }

        public void Invoke(TextWriter cout)
        {
            foreach (var item in Invoke())
            {
                cout.WriteLine(item);
            }
        }
        public virtual string InvokeToJson()
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<string> Invoke() 
        {
            foreach (var argument in RecognizedArguments.Where(argument => null != argument.Argument.Action))
            {
                argument.Argument.Action(argument.Value);
            }
            return new string[0];
        }

        public ParsedArguments Merge(ParsedArguments args)
        {
            return Merge(this, args);
        }

        public static ParsedArguments Merge(ParsedArguments first, ParsedArguments second)
        {
            return new MergedParsedArguments(first, second);
        }

        public IEnumerable<Argument> UnMatchedRequiredArguments()
        {
            var unMatchedRequiredArguments = ArgumentWithOptions
                .Where(argumentWithOptions => argumentWithOptions.Required)
                .Where(argumentWithOptions => !RecognizedArguments
                                                   .Any(recogn => recogn.Argument.Equals(argumentWithOptions)));
            return unMatchedRequiredArguments;
        }

        public void AssertFailOnUnMatched()
        {
            var unMatchedRequiredArguments = UnMatchedRequiredArguments();

            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments")
                          {
                              Arguments = unMatchedRequiredArguments
                                  .Select(unmatched =>unmatched.Name)
                                  .ToArray()
                          };
            }
        }
        public IEnumerable<KeyValuePair<string,string>> RecognizedArgumentsAsKeyValuePairs(){
            return RecognizedArguments.Select(a => a.AsKeyValuePair());
        }
    }
}

