#pragma once
#ifndef H_EXPORTS
#define H_EXPORTS

#include <Windows.h>
#define EXPORT extern "C" __declspec(dllexport)
#define CALLCONV __stdcall

// see more at exports_graphics.cpp->MsdgSwapBuffers
//#define MSDG_COMPATIBILITY_GL

extern const wchar_t* Monosand;

struct win_handle
{
    HWND hwnd;
    HDC hdc;
};

using byte = unsigned char;


#endif