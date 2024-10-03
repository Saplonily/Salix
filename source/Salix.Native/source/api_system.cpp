#include "api_system.h"

#include "api_windowing.h"
#include "api_render_context.h"

#include <timeapi.h>
#include <cstdint>


SLX_API s_bool SLX_CALLCONV SLX_Initialize()
{
    timeBeginPeriod(1);

    if (slxWindowingInit())return true;
    return false;
}