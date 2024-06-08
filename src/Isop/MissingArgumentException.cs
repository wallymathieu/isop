using System;
using System.Collections.Generic;

namespace Isop
{
    public class MissingArgumentException : Exception
    {
        /// <summary>
        /// The arguments that are missing
        /// </summary>
        public IReadOnlyCollection<string> Arguments
        {
            get => (IReadOnlyCollection<string>)Data["Arguments"]!;
            set => Data["Arguments"] = value;
        }

        public MissingArgumentException(string message, IReadOnlyCollection<string> arguments) : base(message) { Arguments = arguments; }
    }
}

