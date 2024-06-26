namespace Salix;

/// <summary>
/// Thrown when an internal framework error occurred.
/// Framework <see cref="ErrorCode"/> is stored in <see cref="Exception.HResult"/>.
/// </summary>
public sealed class FrameworkException : Exception
{
    public ErrorCode ErrorCode => (ErrorCode)HResult; 

    public FrameworkException(string message, ErrorCode errorCode, Exception? innerException = null)
        : base($"{message} (ErrorCode.{errorCode})", innerException) // TODO localization
    {
        HResult = (int)errorCode;
    }

    public FrameworkException(ErrorCode errorCode, Exception? innerException = null)
        : base($"ErrorCode.{errorCode}", innerException)
    {
        HResult = (int)errorCode;
    }

    public FrameworkException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}