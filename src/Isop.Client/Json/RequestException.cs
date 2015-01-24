using Isop.Client.Models;
using Newtonsoft.Json;
using System;

namespace Isop.Client.Json
{
    public class RequestException : Exception
    {
        public readonly System.Net.HttpStatusCode HttpStatusCode;
        public readonly string ErrorContent;
        public readonly string ErrorType;
        public IErrorMessage ErrorObject()
        {
            switch (ErrorType)
            {
                case "TypeConversionFailed":
                    return JsonConvert.DeserializeObject<TypeConversionFailed>(ErrorContent);
                case "MissingArgument":
                    return JsonConvert.DeserializeObject<MissingArgument>(ErrorContent);
                default:
                    return null;
            }
        }
        public RequestException(System.Net.HttpStatusCode httpStatusCode, string errorContent, string errorType)
        {
            this.HttpStatusCode = httpStatusCode;
            ErrorContent = errorContent;
            ErrorType = errorType;
        }
    }
}
