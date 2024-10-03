#include "api_error.h"

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include "common.h"


SLX_API ErrorCode SLX_CALLCONV SLX_GetError()
{
    ErrorCode code = lastErrorCode;
    lastErrorCode = ErrorCode::OK;
    return code;
}

SLX_API int32_t SLX_CALLCONV SLX_GetPlatformError()
{
    DWORD err = GetLastError();
    HRESULT hr = HRESULT_FROM_WIN32(err);
    return hr;
}