using Isop.Client.Transfer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Isop.Client.Transfer
{
    public class MissingArgument : IErrorMessage
    {
        public string Message { get; set; }
        public string Argument { get; set; }
    }
}
