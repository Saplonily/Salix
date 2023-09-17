#pragma once
#ifndef H_NATIVE_HANDLE
#define H_NATIVE_HANDLE
#include <windef.h>

struct whandle
{
    HWND hwnd;
    HDC hdc;
    HGLRC hglrc;
};

#endif