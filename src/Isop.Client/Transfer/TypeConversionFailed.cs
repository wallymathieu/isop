using Isop.Client.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isop.Client.Transfer
{
    public class TypeConversionFailed : IErrorMessage
    {
        public string Message { get; set; }
        public string Argument { get; set; }
        public string Value { get; set; }
        public string TargetType { get; set; }
    }
}
