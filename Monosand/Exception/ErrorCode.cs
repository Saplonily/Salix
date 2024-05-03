namespace Monosand;

public enum ErrorCode : int
{
    Unknown = 0,
    None = 10,
    RegisterWindowFailed = 20,
    CreateRenderContextTwice = 30,
    CreateRenderContextFailed = 31,
    ContextNotSupportSwapControl = 32,
    ContextNotSupportDebugOutput = 33,
    CreateWindowFailed = 40,
    ContextAttachFailed = 50,
}