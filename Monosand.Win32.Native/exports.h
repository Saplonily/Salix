#pragma once
#ifndef H_EXPORTS
#define H_EXPORTS

#include <WinUser.h>
#include "whandle.h"
#include "enums.h"
#define EXPORT __declspec(dllexport)
#define CALLCONV __stdcall

// see more at exports_graphics.cpp->MsdgSwapBuffers
//#define MSDG_COMPATIBILITY_GL

extern const wchar_t* Monosand;

// alloc a small block of memory (will NOT be zeroed)
template<class T>
inline T* small_alloc()
{
    // TODO, i think you know why there's a todo
    return new T;
}

// free a small block of memory (alloced by small_alloc)
template<class T>
inline void small_free(T* ptr)
{
    delete ptr;
}

#define GL_CHECK_ERROR {\
GLenum err = glGetError();\
if (err != 0)\
printf("[glGetError] [%s:%d] %s\n", __FUNCTION__, __LINE__, gl_get_error_msg(err));\
}\

#endif