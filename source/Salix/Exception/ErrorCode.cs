namespace Saladim.Salix;

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

    ContextGLInvalidEnum = 0x24,
    ContextGLInvalidValue = 0x25,
    ContextGLInvalidOperation = 0x26,
    ContextGLInvalidFramebufferOperation = 0x27,
    ContextGLOutOfMemory = 0x29,
    ContextGLStackUnderflow = 0x2a,
    ContextGLStackOverflow = 0x2b,
    ContextGLUnknownError = 0x2c,

    GLFramebufferNotComplete = 0x30
}