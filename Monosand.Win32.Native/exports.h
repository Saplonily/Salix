#pragma once
#ifndef H_EXPORTS
#define H_EXPORTS

#include "whandle.h"
#define EXPORT __declspec(dllexport)
#define CALLCONV __stdcall

void ensure_context(whandle* handle);
extern const wchar_t* Monosand;

#endif