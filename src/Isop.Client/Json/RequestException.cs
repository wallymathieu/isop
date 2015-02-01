using Isop.Client.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
namespace Isop.Client.Json
{
    public class RequestException : Exception
    {
        public readonly System.Net.HttpStatusCode HttpStatusCode;
        public readonly string ErrorContent;
        public IErrorMessage[] ErrorObjects()
        {
            return JArray.Parse(ErrorContent).Select(elem => Parse(elem)).ToArray();
        }

        private IErrorMessage Parse(JToken elem)
        {
            var errorType = (Get(elem, "errorType") ?? String.Empty).ToLower();
            var message = Get(elem, "message");
            var argument = Get(elem, "argument");
            switch (errorType)
            {
                case "typeconversionfailed":
                    return new TypeConversionFailed
                    {
                        Message = message,
                        Argument = argument,

                        Value = Get(elem, "value"),
                        TargetType = Get(elem, "targetType")
                    };
                case "missingargument":
                    return new MissingArgument
                    {
                        Message = message,
                        Argument = argument,
                    };
                default:
                    return new AnyFailure
                    {
                        Message = message,
                        Argument = argument,
                        ErrorType = Get(elem, "errorType")
                    };
            }
        }

        private string Get(JToken elem, string name)
        {
            var json = (elem[name] ?? elem[FirstLetterUpperCase(name)]);
            return json != null ? json.Value<string>() : null;
        }

        private string FirstLetterUpperCase(string name)
        {
            return name.Substring(0, 1).ToUpper() + name.Substring(1);
        }

        public RequestException(System.Net.HttpStatusCode httpStatusCode, string errorContent)
        {
            this.HttpStatusCode = httpStatusCode;
            ErrorContent = errorContent;
        }
    }
}
