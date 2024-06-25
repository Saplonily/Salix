#pragma once
#ifndef H_API_GRAPHICS
#define H_API_GRAPHICS

#include <glad/glad.h>
#include "common.h"
#include "error.h"
#include "graphics_enums.h"

struct vertex_type_handle
{
    VertexElementType* type_ptr;
    int length;
    // for the default 'DrawPrimitive' method 
    // TODO: did we really need that?
    GLuint default_vao_id;
};

struct buffer_handle
{
    GLuint vbo_id, vao_id, ibo_id;
};

void graphics_initialize();

SLX_API void SLX_CALLCONV SLX_Viewport(int32_t x, int32_t y, int32_t width, int32_t height);
SLX_API void* SLX_CALLCONV SLX_RegisterVertexType(P_IN VertexElementType* type, int32_t len);
SLX_API s_bool SLX_CALLCONV SLX_CreateVertexBuffer(P_IN vertex_type_handle* vertex_type, s_bool use_ibo, P_OUT buffer_handle** out_handle);
SLX_API s_bool SLX_CALLCONV SLX_DrawPrimitives(P_IN vertex_type_handle* vertex_type, PrimitiveType pt, void* data, int32_t data_size, int32_t vertices_to_draw);
SLX_API s_bool SLX_CALLCONV SLX_DeleteVertexBuffer(P_IN buffer_handle* buffer);
SLX_API s_bool SLX_CALLCONV SLX_SetVertexBufferData(buffer_handle* buffer_handle, void* data, int32_t dataSize, VertexBufferDataUsage data_usage);
SLX_API s_bool SLX_CALLCONV SLX_DrawBufferPrimitives(buffer_handle* buffer_handle, PrimitiveType primitiveType, int32_t verticesCount);
SLX_API s_bool SLX_CALLCONV SLX_SetIndexBufferData(buffer_handle* buffer_handle, void* data, int32_t dataSize, VertexBufferDataUsage data_usage);
SLX_API s_bool SLX_CALLCONV SLX_DrawIndexedBufferPrimitives(buffer_handle* buffer_handle, PrimitiveType primitiveType, int32_t verticesCount);
SLX_API s_bool SLX_CALLCONV SLX_CreateTexture(int32_t width, int32_t height, P_OUT void** out_texture);
SLX_API s_bool SLX_CALLCONV SLX_SetTextureFilter(void* tex_handle, TextureFilterType min, TextureFilterType max);
SLX_API s_bool SLX_CALLCONV SLX_SetTextureWrap(void* tex_handle, TextureWrapType wrap);
SLX_API s_bool SLX_CALLCONV SLX_SetTextureData(void* tex_handle, int32_t width, int32_t height, void* data, ImageFormat imageFormat);
SLX_API s_bool SLX_CALLCONV SLX_DeleteTexture(void* tex_handle);
SLX_API s_bool SLX_CALLCONV SLX_SetTexture(int32_t index, void* tex_handle);
SLX_API s_bool SLX_CALLCONV SLX_CreateShaderFromGlsl(const char* vert_source, const char* frag_source, P_OUT void** out_program);
SLX_API s_bool SLX_CALLCONV SLX_DeleteShader(void* shader_handle);
SLX_API s_bool SLX_CALLCONV SLX_SetShader(void* shader_handle);
SLX_API s_bool SLX_CALLCONV SLX_CreateSampler(TextureFilterType filter_type, TextureWrapType wrap_type, P_OUT void** out_sampler);
SLX_API s_bool SLX_CALLCONV SLX_SetSampler(int32_t index, void* sampler_handle);
SLX_API s_bool SLX_CALLCONV SLX_DeleteSampler(void* sampler_handle);
SLX_API int SLX_CALLCONV SLX_GetShaderParamLocation(void* shaderHandle, const char* nameUtf8);
SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamInt(int32_t loc, int32_t value);
SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamFloat(int32_t loc, float value);
SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamVec4(int32_t loc, float* vec);
SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamMat4(int32_t loc, float* mat);
SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamMat3x2(int32_t loc, float* mat);
SLX_API s_bool SLX_CALLCONV SLX_CreateRenderTarget(void* tex_handle, P_OUT void** out_rt);
SLX_API s_bool SLX_CALLCONV SLX_SetRenderTarget(void* fbo_handle);
SLX_API s_bool SLX_CALLCONV SLX_DeleteRenderTarget(void* fbo_handle);

#endif