#pragma once
#ifndef H_API_RENDER_CONTEXT
#define H_API_RENDER_CONTEXT

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include "api_windowing.h"
#include "common.h"
#include "error.h"

s_bool slxapi_render_context_init();

#ifdef SLX_DEBUG
#include <glad/glad.h>
void APIENTRY gl_debug_callback(GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar* msg, const void* userParam);
#endif

struct opengl_render_context;
SLX_API opengl_render_context* SLX_CALLCONV SLX_CreateRenderContext();
SLX_API s_bool SLX_CALLCONV SLX_AttachRenderContext(P_IN msd_window* win, P_IN opengl_render_context* hglrc);
SLX_API void SLX_CALLCONV SLX_SwapBuffers(P_IN msd_window* win);
SLX_API double SLX_CALLCONV SLX_GetVSyncFrameTime();
SLX_API void SLX_CALLCONV SLX_SetVSyncEnabled(s_bool enable);

#endif