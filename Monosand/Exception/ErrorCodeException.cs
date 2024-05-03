using System.Runtime.InteropServices;

namespace Monosand;

public sealed class ErrorCodeException : Exception
{
    public ErrorCode ErrorCode { get; private set; }

    public ErrorCodeException(ErrorCode errorCode, int platformResult = 0)
        : base($"Error: {errorCode}.", platformResult != 0 ? Marshal.GetExceptionForHR(platformResult) : null)
    {
        ErrorCode = errorCode;
        HResult = platformResult;
    }
}
