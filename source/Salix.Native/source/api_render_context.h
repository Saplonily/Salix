#pragma once
#ifndef H_API_RENDER_CONTEXT
#define H_API_RENDER_CONTEXT

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include "api_windowing.h"
#include "common.h"
#include "error.h"

struct render_context_info
{
    int32_t max_texture_units;
};

#ifdef SLX_DEBUG
#include <glad/glad.h>
void APIENTRY glDebugCallbackHandler(GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar* msg, const void* userParam);
#endif

SLX_API HGLRC SLX_CALLCONV SLX_CreateRenderContext(P_IN slxWindow* win);
SLX_API void SLX_CALLCONV SLX_SwapBuffers(P_IN slxWindow* win);
SLX_API double SLX_CALLCONV SLX_GetVSyncFrameTime();
SLX_API void SLX_CALLCONV SLX_SetVSyncEnabled(s_bool enable);
SLX_API s_bool SLX_CALLCONV SLX_QueryRenderContextInfo(P_OUT render_context_info* out_render_context_info);

#endif