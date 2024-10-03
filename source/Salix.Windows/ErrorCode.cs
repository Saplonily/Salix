using System;

namespace Saladim.Salix.Windows;

// error.h
enum ErrorCode : int
{
    OK = 0x000,
    InvalidParameter = 0x001,
    NullParameter = 0x002,
    MapEnumFailed = 0x003,

    PlatformError = 0x100,
    GraphicsApiError = 0x101,
    RegisterWindowFailed = 0x102,

    ContextCreatedTwice = 0x200,
    ContextGLLoadFailed = 0x201,

    ContextGLDebugOutputNotSupported = 0x202,
    ContextSwapControlNotSupported = 0x203,
    ContextWGLCreateContextNotSupported = 0x204,
    ContextWGLPixelFormatNotSupported = 0x205,

    ContextGLInvalidEnum = 0x304,
    ContextGLInvalidValue = 0x305,
    ContextGLInvalidOperation = 0x306,
    ContextGLInvalidFramebufferOperation = 0x307,
    ContextGLOutOfMemory = 0x308,
    ContextGLStackUnderflow = 0x309,
    ContextGLStackOverflow = 0x30a,
    ContextGLUnknownError = 0x30b,
    ContextGLFramebufferNotComplete = 0x30c
};