using System.Runtime.Serialization;

namespace Monosand.Win32;

public class OperationFailedException : Exception
{
    public OperationFailedException()
    {
    }

    public OperationFailedException(string? message) : base(message)
    {
    }

    public OperationFailedException(string? message, Exception? innerException) 
        : base(message, innerException)
    {
    }
}