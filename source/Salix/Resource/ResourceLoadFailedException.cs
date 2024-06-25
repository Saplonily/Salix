namespace Salix;

public class ResourceLoadFailedException : Exception
{
    public ResourceLoadFailedException(Type type)
        : this(null, type, null)
    {
    }

    public ResourceLoadFailedException(string? message, Type type)
        : this(message, type, null)
    {
    }

    public ResourceLoadFailedException(string? message, Type type, Exception? innerExcpetion)
        : base(string.Format(SR.ResourceLoadFailed, type, message), innerExcpetion)
    { }
}