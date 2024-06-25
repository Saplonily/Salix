namespace Salix;

// error.h
public enum ErrorCode : int
{
    OK = 0,
    InvalidParameter = 0x01,
    NullParameter = 0x02,
    EnumMappingFailed = 0x03,

    PlatformError = 0x10,
    GraphicsApiError = 0x11,
    RegisterWindowFailed = 0x12,

    ContextCreatedTwice = 0x20,
    ContextGLLoadFailed = 0x21,
    ContextGLSwapControlNotSupported = 0x22,
    ContextGLDebugOutputNotSupported = 0x23,

    ContextGlInvalidEnum = 0x24,
    ContextGlInvalidValue = 0x25,
    ContextGlInvalidOperation = 0x26,
    ContextGlInvalidFramebufferOperation = 0x27,
    ContextGlOutOfMemory = 0x29,
    ContextGlStackUnderflow = 0x2a,
    ContextGlStackOverflow = 0x2b,
    ContextGlUnknownError = 0x2c,

    GLFramebufferNotComplete = 0x30
}