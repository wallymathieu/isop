using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isop.Client.Models
{
    public class AnyFailure : IErrorMessage
    {
        public string Message { get; set; }

        public string Argument { get; set; }

        public string ErrorType { get; set; }
    }
}
