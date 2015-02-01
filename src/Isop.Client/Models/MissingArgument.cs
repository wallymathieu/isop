﻿using Isop.Client.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Isop.Client.Models
{
    public class MissingArgument : IErrorMessage
    {
        public string Message { get; set; }
        public string Argument { get; set; }
    }
}
