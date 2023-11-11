#pragma once
#ifndef H_EXPORTS
#define H_EXPORTS

#include <Windows.h>
#include "enums.h"
#define EXPORT extern "C" __declspec(dllexport)
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

struct win_handle
{
    HWND hwnd;
    HDC hdc;
};

using byte = unsigned char;


#endif