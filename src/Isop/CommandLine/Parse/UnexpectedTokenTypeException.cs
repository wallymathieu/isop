using System;

namespace Isop.CommandLine.Parse;

internal sealed class UnexpectedTokenTypeException : Exception
{
    public UnexpectedTokenTypeException()
    {
    }

    public UnexpectedTokenTypeException(string message) : base(message)
    {
    }

    public UnexpectedTokenTypeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

