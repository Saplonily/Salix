#include "error.h"

error_code last_error_code;

void slx_set_last_error(error_code error_code)
{
#ifdef SLX_DEBUG
    if (last_error_code != error_code::ok)
        printf("[set_last_error] WARNING: overwriting last error %d.\n", last_error_code);
#endif
    last_error_code = error_code;
}