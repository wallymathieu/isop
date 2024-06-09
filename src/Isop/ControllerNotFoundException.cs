using System;

namespace Isop
{
    public class ControllerNotFoundException : Exception
    {
        public ControllerNotFoundException()
        {
        }

        public ControllerNotFoundException(string message) : base(message)
        {
        }

        public ControllerNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

