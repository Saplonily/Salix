#pragma once
#ifndef H_ERROR_CODE
#define H_ERROR_CODE

#include <cstdint>

enum class error_code : int32_t
{
    ok = 0,
    invalid_parameter = 0x01,
    null_parameter = 0x02,
    enum_mapping_failed = 0x03,

    platform_error = 0x10,
    graphics_api_error = 0x11,
    register_window_failed = 0x12,

    context_created_twice = 0x20,
    context_gl_load_failed = 0x21,
    context_gl_swap_control_not_supported = 0x22,
    context_gl_debug_output_not_supported = 0x23,

    context_gl_invalid_enum = 0x24,
    context_gl_invalid_value = 0x25,
    context_gl_invalid_operation = 0x26,
    context_gl_invalid_framebuffer_operation = 0x27,
    context_gl_out_of_memory = 0x29,
    context_gl_stack_underflow = 0x2a,
    context_gl_stack_overflow = 0x2b,
    context_gl_unknown_error = 0x2c,
    
    gl_framebuffer_not_complete = 0x30
};

extern error_code last_error_code;
void slx_set_last_error(error_code error_code);

#define SLX_FAIL(code) { slx_set_last_error(code); return true; }
#define SLX_FAIL_COND(cond, code) { if(cond) SLX_FAIL(code); }
#define SLX_FAIL_NULL(code) { slx_set_last_error(code); return nullptr; }
#define SLX_FAIL_COND_NULL(cond, code) { if(cond) SLX_FAIL_NULL(code); }
#define SLX_FAIL_GOTO(code, label) { slx_set_last_error(code); goto label; }
#define SLX_FAIL_COND_GOTO(cond, code, label) { if(cond) SLX_FAIL_GOTO(code, label); }

#endif