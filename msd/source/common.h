#pragma once
#ifndef H_COMMON
#define H_COMMON

#define EXPORT extern "C" __declspec(dllexport)
#define CALLCONV __stdcall

constexpr const wchar_t* Monosand = L"Monosand";

using byte = unsigned char;

#endif