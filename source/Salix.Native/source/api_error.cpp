#include "api_error.h"
#include "common.h"


SLX_API error_code SLX_CALLCONV SLX_GetError()
{
    error_code code = last_error_code;
    last_error_code = error_code::ok;
    return code;
}