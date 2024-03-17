namespace Monosand;

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