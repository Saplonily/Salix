namespace Saladim.Salix;

/// <summary>
/// Thrown when an internal framework error occurred.
/// </summary>
public class FrameworkException : Exception
{
    internal FrameworkException(string message, Exception? innerException)
        : base(string.Format(SR.FrameworkExceptionMessage, message), innerException)
    { }

    internal FrameworkException(string message) : this(message, null)
    { }
}