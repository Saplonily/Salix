#pragma once
#ifndef H_API_SYSTEM
#define H_API_SYSTEM

#include "common.h"
#include "error.h"

SLX_API s_bool SLX_CALLCONV SLX_Initialize();
// do we really need this in the unmanged side?
SLX_API int64_t SLX_CALLCONV SLX_GetUsecTimeline(); 

#endif