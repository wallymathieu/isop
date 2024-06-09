namespace Isop;

public class ControllerOrActionNotFoundException : Exception
{
    public ControllerOrActionNotFoundException()
    {
    }

    public ControllerOrActionNotFoundException(string message) : base(message)
    {
    }

    public ControllerOrActionNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}