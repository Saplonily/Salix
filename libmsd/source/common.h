#pragma once
#ifndef H_COMMON
#define H_COMMON

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#define EXPORT extern "C" __declspec(dllexport)
#define CALLCONV __stdcall

constexpr wchar_t* Monosand = L"Monosand";

using byte = unsigned char;

struct win_handle
{
    HWND hwnd;
    HDC hdc;
};

#endif