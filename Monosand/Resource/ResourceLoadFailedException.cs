namespace Monosand;

public class ResourceLoadFailedException : Exception
{
    public string? Resource { get; internal set; }
    public Type? Type { get; internal set; }

    public ResourceLoadFailedException(Type type)
        : this(null, type)
    {
    }

    public ResourceLoadFailedException(string? message, Type type)
        : this(message, type, null)
    {
    }

    public ResourceLoadFailedException(string? message, Type type, Exception? innerExcpetion)
        : base($"Resource type of {type} load failed. {message}", innerExcpetion)
    { }
}