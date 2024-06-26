#include "api_system.h"

#include "api_windowing.h"
#include "api_render_context.h"

#include <timeapi.h>
#include <cstdint>

static int64_t performanceFrequency = 0;

SLX_API s_bool SLX_CALLCONV SLX_Initialize()
{
    QueryPerformanceFrequency((LARGE_INTEGER*)&performanceFrequency);

    timeBeginPeriod(1);

    s_bool r = slxapi_windowing_init();
    if (r) return true;
    r = slxapi_render_context_init();
    if (r) return true;
    return false;
}

SLX_API int64_t SLX_CALLCONV SLX_GetUsecTimeline()
{
    int64_t ticks = 0;
    QueryPerformanceCounter((LARGE_INTEGER*)&ticks);

    int64_t seconds = ticks / performanceFrequency;
    int64_t leftover = ticks % performanceFrequency;
    int64_t time = (leftover * 1000000L) / performanceFrequency;
    time += seconds * 1000000L;
    return time;
}