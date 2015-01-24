using Isop.Client.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Isop.Client.Models
{
    public class MissingArgument : IErrorMessage
    {
        public string Message { get; set; }
        public IList<string> Arguments { get; set; }
    }
}
