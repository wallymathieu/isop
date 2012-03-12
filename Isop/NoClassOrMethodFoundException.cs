using System;

namespace Isop
{
    public class NoClassOrMethodFoundException : Exception
    {
        public NoClassOrMethodFoundException() { }

        public NoClassOrMethodFoundException(string message) : base(message) { }

        public NoClassOrMethodFoundException(string message, Exception inner) : base(message, inner) { }
    }
}

