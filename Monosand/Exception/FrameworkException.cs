namespace Monosand;

/// <summary>Thrown when an internal framework error occurred.</summary>
public class FrameworkException : Exception
{
    public FrameworkException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
