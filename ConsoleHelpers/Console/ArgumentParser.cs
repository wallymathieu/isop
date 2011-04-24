using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Helpers.Console
{
    public class MissingArgumentException : Exception
    {
        public List<string> Arguments;
        public MissingArgumentException() { }

        public MissingArgumentException(string message) : base(message) { }

        public MissingArgumentException(string message, Exception inner) : base(message, inner) { }
    }

    public class Argument
    {
        public static implicit operator Argument(string value)
        {
            return Parse(value);
        }

        public static Argument Parse(string value)
        {
            OptionArgument optionArgument;
            if (OptionArgument.TryParse(value, out optionArgument))
                return optionArgument;
            VisualStudioArgument visualStudioArgument;
            if (VisualStudioArgument.TryParse(value, out visualStudioArgument))
                return visualStudioArgument;
            throw new NotImplementedException(value);
        }

        public string Prototype { get; protected set; }
        public string[] Aliases { get; protected set; }
        public string Delimiter { get; protected set; }

        public override string ToString()
        {
            return Prototype;
        }
    }

    public class OptionArgument : Argument
    {
        public OptionArgument(string prototype, string[] names, string delimiter)
        {
            Prototype = prototype;
            Aliases = names;
            Delimiter = delimiter;
        }

        public static bool TryParse(string value, out OptionArgument optionArgument)
        {
            if (value.Contains("|"))
            {
                var prototype = value;
                var names = prototype.TrimEnd('=', ':').Split('|');
                string delimiter = null;
                var last = prototype.Last();
                switch (last)
                {
                    case '=':
                    case ':':
                        delimiter = last.ToString();
                        break;
                    default:
                        break;
                }
                optionArgument = new OptionArgument(prototype, names, delimiter);
                return true;
            }
            optionArgument = null;
            return false;
        }
    }

    /// <summary>
    /// Represents the argument. For instance "file" of the commandline argument --file. 
    /// Usually you might want it to recognize -f as well. For instance using Argument.Parse("&file"), or the implicit 
    /// string cast operator.
    /// </summary>
    public class VisualStudioArgument : Argument
    {
        /// <summary>
        /// same pattern as in visual studio external tools: &amp;tool
        /// </summary>
        public static readonly Regex VisualStudioArgPattern = new Regex(@"(\&?)(.)[^=:]*([=:]?)");

        public static bool TryParse(string value, out VisualStudioArgument visualStudioArgument)
        {
            //TODO: need to do some cleaning here
            var match = VisualStudioArgPattern.Match(value);
            if (match.Success)
            {
                var aliases = new List<string>();
                string val;
                if (match.Groups[1].Length > 0)
                {
                    val = value.Replace("&", "");
                    if (match.Groups[2].Length > 0)
                        aliases.Add(match.Groups[2].Value);
                }
                else
                {
                    val = value;
                }
                string delimiter;
                if (match.Groups[3].Length > 0)
                {
                    delimiter = match.Groups[3].Value;
                    val = val.Replace(delimiter, "");
                }
                else delimiter = null;
                aliases.Add(val);

                visualStudioArgument = new VisualStudioArgument
                {
                    Prototype = value,
                    Aliases = aliases.ToArray(),
                    Delimiter = delimiter
                };
                return true;
            }
            visualStudioArgument = null;
            return false;
        }
    }
    /// <summary>
    /// class to enable extensions of the behavior of what is recognized as arguments.
    /// </summary>
    public class ArgumentWithOptions
    {
        public Argument Argument { get; private set; }
        public Action<string> Action { get; set; }
        public bool Required { get; set; }

        public ArgumentWithOptions(Argument argument, Action<string> action = null, bool required = false)
        {
            Argument = argument;
            Action = action;
            Required = required;
        }

        public bool HasAlias(string value)
        {
            return Argument.Aliases.Any(alias=> value.Equals(alias,StringComparison.OrdinalIgnoreCase));
        }
    }

    public class RecognizedArgument
    {
        /// <summary>
        /// the matched value if any, for instance the "value" of the expression "--argument value"
        /// </summary>
        public string Value { get; private set; }
        public ArgumentWithOptions WithOptions { get; private set; }
        /// <summary>
        /// the "argument" of the expression "--argument"
        /// </summary>
        public string Argument { get; private set; }

        public RecognizedArgument(ArgumentWithOptions argumentWithOptions, string parameter, string value = null)
        {
            Value = value;
            WithOptions = argumentWithOptions;
            Argument = parameter;
        }
    }
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

        public IEnumerable<string> UnRecognizedArguments { get; set; }

        public IEnumerable<ArgumentWithOptions> ArgumentWithOptions { get; set; }

        public virtual void Invoke()
        {
            foreach (var argument in RecognizedArguments.Where(argument => null != argument.WithOptions.Action))
            {
                argument.WithOptions.Action(argument.Value);
            }
        }
    }
    public class ArgumentParser
    {
        public static ArgumentParserBuilder Build() { return new ArgumentParserBuilder(); }
        private readonly IEnumerable<ArgumentWithOptions> _argumentWithOptions;

        public ArgumentParser(IEnumerable<ArgumentWithOptions> argumentWithOptions)
        {
            _argumentWithOptions = argumentWithOptions;
        }

        public ParsedArguments Parse(IEnumerable<string> arguments)
        {
            var lexer = new ArgumentLexer(arguments);
            var recognizedIndexes=new List<int>();
            
            IList<RecognizedArgument> recognized=new List<RecognizedArgument>();
            while (lexer.HasMore())
            {
                var current = lexer.Next();
                switch (current.TokenType)
                {
                    case TokenType.Argument:
                        break;
                    case TokenType.Parameter:
                        var argumentWithOptions = _argumentWithOptions
                            .SingleOrDefault(argopt=> argopt.HasAlias(current.Value));
                        if (null == argumentWithOptions)
                            continue;                        
                        string value;
                        recognizedIndexes.Add(current.Index);
                        if (lexer.Peek().TokenType==TokenType.ParameterValue)
                        {
                            var paramValue = lexer.Next();
                            recognizedIndexes.Add(paramValue.Index);
                            value=paramValue.Value;
                        }
                        else
                        {
                            value = string.Empty;
                        }

                        recognized.Add(new RecognizedArgument(
                                    argumentWithOptions,
                                    current.Value,
                                    value));
                        break;
                    case TokenType.ParameterValue:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(current.TokenType.ToString());
                }
            }

            var argumentList = arguments.ToList();

            var unRecognizedArguments = argumentList
                .Select((value, i) => new { i, value })
                .Where(indexAndValue => !recognizedIndexes.Contains(indexAndValue.i))
                .Select(v => v.value);

            var unMatchedRequiredArguments = _argumentWithOptions.Where(argumentWithOptions => argumentWithOptions.Required)
                .Where(argumentWithOptions => !recognized.Any(recogn => recogn.WithOptions.Equals(argumentWithOptions)));
            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments") { Arguments = unMatchedRequiredArguments.Select(unmatched => unmatched.Argument.ToString()).ToList() };
            }
            return new ParsedArguments
            {
                ArgumentWithOptions = _argumentWithOptions.ToArray(),
                RecognizedArguments = recognized,
                UnRecognizedArguments = unRecognizedArguments
            };
        }
    }
}
