#pragma once
#ifndef H_EXPORTS
#define H_EXPORTS

#include <WinUser.h>
#include "whandle.h"
#define EXPORT __declspec(dllexport)
#define CALLCONV __stdcall

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

#endif