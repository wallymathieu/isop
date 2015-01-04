using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isop.Gui
{
    public class RequestError 
    {
        public readonly System.Net.HttpStatusCode HttpStatusCode;
        public readonly string Message;
        public readonly string ErrorType;

        public RequestError(System.Net.HttpStatusCode httpStatusCode, string message, string errorType)
        {
            this.HttpStatusCode = httpStatusCode;
            Message = message;
            ErrorType = errorType;
        }
    }
}
