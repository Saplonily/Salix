#pragma once
#ifndef H_ERROR
#define H_ERROR

#include <cstdint>

enum class ErrorCode : int32_t
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

extern ErrorCode lastErrorCode;
void slxSetLastError(ErrorCode errorCode);

#define SLX_FAIL(code) { slxSetLastError(code); return true; }
#define SLX_FAIL_COND(cond, code) { if (cond) SLX_FAIL(code); }
#define SLX_FAIL_NULL(code) { slxSetLastError(code); return nullptr; }
#define SLX_FAIL_COND_NULL(cond, code) { if (cond) SLX_FAIL_NULL(code); }
#define SLX_FAIL_RET(code, ret) { slxSetLastError(code); return ret; }
#define SLX_FAIL_COND_RET(cond, code, ret) { if (cond) SLX_FAIL_RET(code, ret); }
#define SLX_FAIL_GOTO(code, label) { slxSetLastError(code); goto label; }
#define SLX_FAIL_COND_GOTO(cond, code, label) { if (cond) SLX_FAIL_GOTO(code, label); }
#define SLX_FAIL_MAP_ENUM() { assert(false); SLX_FAIL_RET(ErrorCode::MapEnumFailed, -1); }
#define SLX_FAIL_MAP_ENUM_RET(ret) { assert(false); SLX_FAIL_RET(ErrorCode::MapEnumFailed, ret); }

#endif