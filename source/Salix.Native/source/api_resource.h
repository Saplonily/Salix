#pragma once
#ifndef H_API_RESOURCE
#define H_API_RESOURCE

#include "graphics_enums.h"
#include "common.h"

SLX_API void* SLX_CALLCONV SLX_LoadImage(void* mem, int length, P_OUT int* x, P_OUT int* y, P_OUT int* data_length, P_OUT ImageFormat* format);
SLX_API void SLX_CALLCONV SLX_FreeImage(void* texData);

#endif