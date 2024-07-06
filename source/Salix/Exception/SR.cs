namespace Saladim.Salix;

internal static class SR
{
    public static readonly string KeyNotPresent = "The given key '{0}' was not present in the dictionary.";
    public static readonly string KeyNotPresentButAlive = "The given key '{0}' was present in the dictionary but it was not alive.";
    public static readonly string ValueCannotBeNegative = "Value can not be negative.";
    public static readonly string ValueMustBePositive = "Value must be greater then 0.";
    public static readonly string FailedToCreateRenderContext = "Failed to create RenderContext.";
    public static readonly string TypeNotSupportedInShader = "Type of '{0}' is not supported in shader parameter.";
    public static readonly string PlatformInitializeFailed = "Platform initialize failed.";
    public static readonly string ResourceTypeNotSupported = "Resource type {0} is not supported.";
    public static readonly string StreamIsTooLong = "The stream is too long.";
    public static readonly string InvalidStreamLength = "Invalid stream length.";
    public static readonly string InvalidImageData = "Invalid image data.";
    public static readonly string FailedToCreateWindow = "Failed to create window.";
    public static readonly string TooManyWindowEvents = "Too many window events. (> int.MaxValue)";
    public static readonly string PreciseTooSmall = "Precise is too small. (less than 3)";
    public static readonly string PreciseTooBig = "Precise is too big. (greater than 8192)";
    public static readonly string BufferIsIndexed = "This buffer is indexed.";
    public static readonly string BufferIsNotIndexed = "This buffer is not indexed.";
    public static readonly string UnmatchedShaderParamOwner = "Unmatched shader of ShaderParameter.";
    public static readonly string ImageDataIsNull = "Image data is null.";
    public static readonly string VerticesDataIsNull = "Vertices or indices data is null";
    public static readonly string UnknownWindowEventType = "Unknown window event type {0}.";
    public static readonly string ResourceLoadFailed = "Resource type of {0} load failed. {1}";
    public static readonly string InvalidWindowSize = "Invalid window size.";
    public static readonly string FailedToAttachRenderContext = "Failed to attach RenderContext.";
    public static readonly string ThrowOnOK = "Attempt to throw FrameworkException on ErrorCode OK, if this is not expected please report this bug.";
    public static readonly string FailedToGetWindowTitle = "Failed to get the title of the window.";
    public static readonly string ShaderParamNotFound = "Shader parameter '{0}' does not exist.";
}