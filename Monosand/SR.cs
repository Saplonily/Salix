namespace Monosand;

internal static class SR
{
    public static readonly string KeyNotPresent = "The given key '{0}' was not present in the dictionary.";
    public static readonly string KeyNotPresentButAlive = "The given key '{0}' was present in the dictionary but it was not alive.";
    public static readonly string ValueCannotBeNegative = "Value can not be negative.";
    public static readonly string FailedToCreateRenderContext = "Failed to create RenderContext.";
    public static readonly string TypeNotSupportedInShader = "Type of '{0}' is not supported in shader parameter.";
    public static readonly string ShaderRequiredCurrent = "This operation required this shader to be the current.";
    public static readonly string PreciseTooSmall = "Precise is less than 3.";
    public static readonly string PlatformInitializeFailed = "Platform initialize failed.";
    public static readonly string ResourceTypeNotSupported = "Resource type {0} is not supported.";
    public static readonly string StreamIsTooLong = "The stream is too long.";
    public static readonly string InvalidStreamLength = "Invalid stream length.";
    public static readonly string InvalidImageData = "Invalid image data.";
    public static readonly string FailedToCreateWindow = "Failed to create window.";
    public static readonly string TooManyWindowEvents = "Too many window events. (> int.MaxValue)";
}