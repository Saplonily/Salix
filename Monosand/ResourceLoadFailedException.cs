namespace Monosand;

public class ResourceLoadFailedException : Exception
{
    public string? Resource { get; internal set; }
    public ResourceType Type { get; internal set; }

    public ResourceLoadFailedException()
    {
    }

    public ResourceLoadFailedException(string? message) : base(message)
    {
    }

    public ResourceLoadFailedException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}