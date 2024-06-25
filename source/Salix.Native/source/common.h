#pragma once
#ifndef H_COMMON
#define H_COMMON

#include <cstdint>

#ifdef __GNUC__
#define SLX_COMPILER_GCC
#endif // __GNUC__

#ifdef __MINGW32__
#define SLX_COMPILER_MINGW
#endif // __MINGW32__

#ifdef _MSC_VER
#define SLX_COMPILER_MSVC
#endif // _MSC_VER

#ifdef SLX_COMPILER_MSVC
#define SLX_API extern "C" __declspec(dllexport)
#define SLX_CALLCONV __stdcall
#else
#error TODO
#endif

#define P_OUT
#define P_IN
#define P_INOUT

#ifdef SLX_COMPILER_MSVC
#ifdef _DEBUG
#define SLX_DEBUG
#else
#define SLX_RELEASE
#endif // _DEBUG

#endif // SLX_COMPILER_MSVC

#if (!defined SLX_DEBUG) && (!defined SLX_RELEASE)
#define SLX_RELEASE
#endif // !SLX_DEBUG or SLX_RELEASE

constexpr const wchar_t* FrameworkName = L"Saladim.Salix";

using s_byte = uint8_t;
using s_bool = uint8_t;

#endif // H_COMMON