#include "error.h"

#include <stdio.h>

#include "common.h"

ErrorCode lastErrorCode;

void slxSetLastError(ErrorCode errorCode)
{
#ifdef SLX_DEBUG
    if (lastErrorCode != ErrorCode::OK)
        printf("[set_last_error] WARNING: overwriting last error %d.\n", lastErrorCode);
#endif
    lastErrorCode = errorCode;
}