using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
namespace Isop
{
    public class MissingArgumentException : Exception
    {
        /// <summary>
        /// The arguments. The key are the argument, the value is the description or help.
        /// </summary>
        public List<KeyValuePair<string, string>> Arguments;
        public MissingArgumentException() { }

        public MissingArgumentException(string message) : base(message) { }

        public MissingArgumentException(string message, Exception inner) : base(message, inner) { }
    }

    public class NoClassOrMethodFoundException : Exception
    {
        public NoClassOrMethodFoundException() { }

        public NoClassOrMethodFoundException(string message) : base(message) { }

        public NoClassOrMethodFoundException(string message, Exception inner) : base(message, inner) { }
    }

    public class ArgumentParameter
    {
        public string Prototype { get; protected set; }
        public int? Ordinal { get; protected set; }

        public static implicit operator ArgumentParameter(string value)
        {
            return Parse(value, CultureInfo.CurrentCulture);
        }

        public static ArgumentParameter Parse(string value, IFormatProvider formatProvider)
        {
            OrdinalParameter ordinalParameter;
            if (OrdinalParameter.TryParse(value, formatProvider, out ordinalParameter))
                return ordinalParameter;
            OptionParameter optionParameter;
            if (OptionParameter.TryParse(value, out optionParameter))
                return optionParameter;
            VisualStudioParameter visualStudioParameter;
            if (VisualStudioParameter.TryParse(value, out visualStudioParameter))
                return visualStudioParameter;
            throw new ArgumentOutOfRangeException(value);
        }

        public string[] Aliases { get; protected set; }
        public string Delimiter { get; protected set; }
        public string Help()
        {
            return "--" + string.Join(", or ", Aliases)
                + (string.IsNullOrEmpty(Delimiter)
                    ? ""
                    : " " + Delimiter)
                    ;
        }
        public override string ToString()
        {
            return Prototype;
        }

    }
    public class OrdinalParameter : ArgumentParameter
    {
        public OrdinalParameter(string prototype, string[] names, string delimiter, int ordinal)
        {
            Ordinal = ordinal;
            Prototype = prototype;
            Aliases = names;
            Delimiter = delimiter;
        }
        private static readonly Regex Pattern = new Regex(@"#(?<ord>\d*)(?<rest>.*)");
        public static bool TryParse(string value, IFormatProvider formatProvider, out OrdinalParameter ordinalParameter)
        {
            var match = Pattern.Match(value);
            if (match.Success)
            {
                var prototype = value;
                var rest = match.Groups["rest"].Value;
                var param = ArgumentParameter.Parse(rest, formatProvider);
                ordinalParameter = new OrdinalParameter(prototype, param.Aliases, param.Delimiter, int.Parse(match.Groups["ord"].Value, formatProvider));
                return true;
            }
            ordinalParameter = null;
            return false;
        }
    }

    /// <summary>
    /// Represents the parameter. For instance "file" of the commandline argument --file. 
    /// Usually you might want it to recognize -f as well. For instance using Argument.Parse("file|f"), or the implicit 
    /// string cast operator.
    /// </summary>
    public class OptionParameter : ArgumentParameter
    {
        public OptionParameter(string prototype, string[] names, string delimiter)
        {
            Prototype = prototype;
            Aliases = names;
            Delimiter = delimiter;
        }
        /// <summary>
        /// Note: this is accept invalid patterns.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="optionParameter"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out OptionParameter optionParameter)
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
                optionParameter = new OptionParameter(prototype, names, delimiter);
                return true;
            }
            optionParameter = null;
            return false;
        }
    }

    /// <summary>
    /// Represents the parameter. For instance "file" of the commandline argument --file. 
    /// Usually you might want it to recognize -f as well. For instance using Argument.Parse("&file"), or the implicit 
    /// string cast operator.
    /// </summary>
    public class VisualStudioParameter : ArgumentParameter
    {
        /// <summary>
        /// same pattern as in visual studio external tools: &amp;tool
        /// </summary>
        public static readonly Regex VisualStudioArgPattern = new Regex(@"(?<prefix>\&?)(?<alias>.)[^=:]*(?<equals>[=:]?)");

        public static bool TryParse(string value, out VisualStudioParameter visualStudioParameter)
        {
            //TODO: need to do some cleaning here
            var match = VisualStudioArgPattern.Match(value);
            if (match.Success)
            {
                var aliases = new List<string>();
                string val;
                if (match.Groups["prefix"].Length > 0)
                {
                    val = value.Replace("&", "");
                    if (match.Groups["alias"].Length > 0)
                        aliases.Add(match.Groups["alias"].Value);
                }
                else
                {
                    val = value;
                }
                string delimiter;
                if (match.Groups["equals"].Length > 0)
                {
                    delimiter = match.Groups["equals"].Value;
                    val = val.Replace(delimiter, "");
                }
                else delimiter = null;
                aliases.Add(val);

                visualStudioParameter = new VisualStudioParameter
                {
                    Prototype = value,
                    Aliases = aliases.ToArray(),
                    Delimiter = delimiter
                };
                return true;
            }
            visualStudioParameter = null;
            return false;
        }
    }
    /// <summary>
    /// class to enable extensions of the behavior of what is recognized as arguments.
    /// </summary>
    public class ArgumentWithOptions
    {
        public string Description { get; private set; }
        public ArgumentParameter Argument { get; private set; }
        public Action<string> Action { get; set; }
        public bool Required { get; set; }

        public ArgumentWithOptions(ArgumentParameter argument, Action<string> action = null, bool required = false, string description = null)
        {
            Description = description;
            Argument = argument;
            Action = action;
            Required = required;
        }
        /// <summary>
        /// todo:move this
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool HasAlias(string value)
        {
            return Argument.Aliases.Any(alias => value.Equals(alias, StringComparison.OrdinalIgnoreCase));
        }

        public string Help()
        {
            return Argument.Help()
                + (String.IsNullOrEmpty(Description)
                    ? ""
                    : "\t" + Description);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ArgumentWithOptions)) return false;
            return Equals((ArgumentWithOptions) obj);
        }

        public bool Equals(ArgumentWithOptions other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Description, Description) && Equals(other.Argument, Argument) && other.Required.Equals(Required);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Description != null ? Description.GetHashCode() : 0);
                result = (result*397) ^ (Argument != null ? Argument.GetHashCode() : 0);
                result = (result*397) ^ Required.GetHashCode();
                return result;
            }
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
        public override int GetHashCode()
        {
            return Argument.GetHashCode() + WithOptions.GetHashCode() + (Value ?? "").GetHashCode() + 1794;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;
            if (obj is RecognizedArgument)
            {
                var rec = obj as RecognizedArgument;
                return Argument.Equals(rec.Argument)
                    && WithOptions.Equals(rec.WithOptions)
                    && string.Equals(Value, rec.Value);
            }
            return false;
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

    public class UnrecognizedArgument
    {
        public int Index { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            return Value;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(UnrecognizedArgument)) return false;
            return Equals((UnrecognizedArgument)obj);
        }

        public bool Equals(UnrecognizedArgument other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Index == Index && Equals(other.Value, Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Index * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }

    public class ArgumentParser
    {
        [Obsolete("Use new Build()...")]
        public static Build Build() { return new Build(); }
        private readonly IEnumerable<ArgumentWithOptions> _argumentWithOptions;

        public ArgumentParser(IEnumerable<ArgumentWithOptions> argumentWithOptions)
        {
            _argumentWithOptions = argumentWithOptions;
        }
        public ParsedArguments Parse(IEnumerable<string> arguments)
        {
            var lexer = new ArgumentLexer(arguments);
            var parsedArguments = Parse(lexer, arguments);
            var unMatchedRequiredArguments = parsedArguments.UnMatchedRequiredArguments();

            if (unMatchedRequiredArguments.Any())
            {
                throw new MissingArgumentException("Missing arguments")
                {
                    Arguments = unMatchedRequiredArguments
                        .Select(unmatched => new KeyValuePair<string, string>(unmatched.Argument.ToString(), unmatched.Argument.Help())).ToList()
                };
            }
            return parsedArguments;
        }

        public ParsedArguments Parse(ArgumentLexer lex, IEnumerable<string> arguments)
        {
            var recognizedIndexes = new List<int>();
            var lexer = new PeekEnumerable<Token>(lex);
            IList<RecognizedArgument> recognized = new List<RecognizedArgument>();
            while (lexer.HasMore())
            {
                var current = lexer.Next();
                switch (current.TokenType)
                {
                    case TokenType.Argument:
                        {
                            // TODO : move recognize into ArgumentBase
                            var argumentWithOptions = _argumentWithOptions
                               .Where(argopt => !argopt.Argument.Ordinal.HasValue)
                               .SingleOrDefault(argopt => argopt.Argument
                                   .Prototype.Equals(current.Value, StringComparison.OrdinalIgnoreCase));
                            if (null == argumentWithOptions)
                            {
                                argumentWithOptions = _argumentWithOptions
                                        .Where(argopt => argopt.Argument.Ordinal.HasValue)
                                        .SingleOrDefault(argopt => argopt.Argument.Ordinal.Value.Equals(current.Index)
                                          && argopt.HasAlias(current.Value));
                                if (null == argumentWithOptions)
                                {
                                    continue;
                                }
                            }
                            recognizedIndexes.Add(current.Index);
                            recognized.Add(new RecognizedArgument(
                                        argumentWithOptions,
                                        current.Value));
                        }
                        break;
                    case TokenType.Parameter:
                        {
                            var argumentWithOptions = _argumentWithOptions
                               .SingleOrDefault(argopt => argopt.HasAlias(current.Value));
                            if (null == argumentWithOptions)
                                continue;
                            string value;
                            recognizedIndexes.Add(current.Index);
                            if (lexer.Peek().TokenType == TokenType.ParameterValue)
                            {
                                var paramValue = lexer.Next();
                                recognizedIndexes.Add(paramValue.Index);
                                value = paramValue.Value;
                            }
                            else
                            {
                                value = string.Empty;
                            }

                            recognized.Add(new RecognizedArgument(
                                        argumentWithOptions,
                                        current.Value,
                                        value));
                        }
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
                .Select(v => new UnrecognizedArgument { Index = v.i, Value = v.value });

            return new ParsedArguments
            {
                ArgumentWithOptions = _argumentWithOptions.ToArray(),
                RecognizedArguments = recognized,
                UnRecognizedArguments = unRecognizedArguments
            };
        }

        public static Build BuildFromConfiguration(Type type)
        {
            return new Build().Configuration(type);
        }
    }
}
