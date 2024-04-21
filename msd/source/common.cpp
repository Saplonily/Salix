#include "common.h"
#include "initializations.h"
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <cstdint>

static int64_t performanceFrequency = 0;

EXPORT int CALLCONV MsdInitialize()
{
    QueryPerformanceFrequency((LARGE_INTEGER*)&performanceFrequency);

    windowing_initialize();

    return 0;
}

EXPORT int64_t CALLCONV MsdGetUsecTimeline()
{
    int64_t ticks = 0;
    QueryPerformanceCounter((LARGE_INTEGER*)&ticks);

    uint64_t seconds = ticks / performanceFrequency;
    uint64_t leftover = ticks % performanceFrequency;
    uint64_t time = (leftover * 1000000L) / performanceFrequency;
    time += seconds * 1000000L;
    return time;
}