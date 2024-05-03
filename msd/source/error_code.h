#pragma once
#ifndef H_ERROR_CODE
#define H_ERROR_CODE

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <cstdint>

enum class error_code : int32_t
{
    unknown = 0,
    none = 10,
    register_window_failed = 20,
    create_render_context_twice = 30,
    create_render_context_failed = 31,
    context_not_support_swap_control = 32,
    context_not_support_debug_output = 33,
    create_window_failed = 40,
    context_attach_failed = 50
};

using platform_result_t = DWORD;

template<typename T>
struct m_result
{
    error_code err;
    platform_result_t platform_result;
    T return_value;
};

template<typename T>
m_result<T> make_result(error_code err, platform_result_t platform_result = (platform_result_t)0, T value = (T)0)
{
    m_result<T> r{};
    r.err = err;
    r.platform_result = platform_result;
    r.return_value = value;
    return r;
}

template<typename T>
m_result<T> make_result(T value)
{
    return make_result(error_code::none, (platform_result_t)0, value);
}

template<>
struct m_result<void>
{
    error_code err;
    platform_result_t platform_result;
};

inline m_result<void> make_result(error_code err, platform_result_t platform_result = (platform_result_t)0)
{
    m_result<void> r{};
    r.err = err;
    r.platform_result = platform_result;
    return r;
}

inline m_result<void> make_result()
{
    return make_result(error_code::none, (platform_result_t)0);
}

#endif