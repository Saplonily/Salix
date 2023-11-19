namespace Monosand;

public class ResourceLoadFailedException : Exception
{
    public string? Resource { get; internal set; }
    public ResourceType Type { get; internal set; }

    public ResourceLoadFailedException(ResourceType type)
        : this(null, type)
    {
    }

    public ResourceLoadFailedException(string? message, ResourceType type)
        : this(message, type, null)
    {
    }

    public ResourceLoadFailedException(string? message, ResourceType type, Exception? innerExcpetion)
        : base($"Resource type of {type} load failed. {message}", innerExcpetion)
    { }
}