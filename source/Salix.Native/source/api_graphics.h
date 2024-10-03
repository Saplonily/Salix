#pragma once
#ifndef H_API_GRAPHICS
#define H_API_GRAPHICS

#include <glad/glad.h>
#undef APIENTRY
#include <cstdint>
#include "common.h"
#include "error.h"

#pragma region enums & mapping

// Salix/Graphics/Vertex/VertexElementType.cs
enum class VertexElementType
{
    Single,
    Color,
    Vector2,
    Vector3,
    Vector4,
};

// Salix/Graphics/Vertex/PrimitiveType.cs
enum class PrimitiveType
{
    TriangleList,
    TriangleStrip,
    TriangleFan,
    LineList,
    LineStrip,
    PointList
};

// Salix/Graphics/ImageFormat.cs
enum class ImageFormat
{
    R8,
    Rg16,
    Rgb24,
    Rgba32
};

// Salix/Graphics/TextureFilterType.cs
enum class TextureFilterType
{
    Linear,
    Nearest,
    LinearMipmapLinear,
    LinearMipmapNearest,
    NearestMipmapLinear,
    NearestMipmapNearest
};

// Salix/Graphics/TextureWrapType.cs
enum class TextureWrapType
{
    ClampToEdge,
    Repeat,
    MirroredRepeat
};

GLsizei slxVertexElementTypeGetSize(VertexElementType type);
struct VertexElementTypeGLSize { int count; GLenum type; int size; };
VertexElementTypeGLSize slxVertexElementTypeGetGLSize(VertexElementType type);
GLenum slxMapPrimitiveType(PrimitiveType type);
GLenum slxMapImageFormat(ImageFormat format);
int slxImageFormatGetSize(ImageFormat format);
GLenum slxMapTextureFilterType(TextureFilterType type);
GLenum slxMapTextureWrapType(TextureWrapType type);
ErrorCode slxMapGLError(GLenum glerr);

#pragma endregion

struct VertexBufferBinding
{
    void* buffer;
    int32_t elementsCount;
    VertexElementType* type;
    int32_t offset;
    int32_t instanceFrequency;
};

void graphics_initialize();

SLX_API s_bool SLX_CALLCONV SLX_Viewport(int32_t x, int32_t y, int32_t width, int32_t height);
SLX_API s_bool SLX_CALLCONV SLX_Clear(float r, float g, float b, float a);

SLX_API void* SLX_CALLCONV SLX_CreateVertexBuffersInput(int32_t count, VertexBufferBinding* bindings);
SLX_API void* SLX_CALLCONV SLX_CreateSingleVertexBufferInput(int32_t elementsCount, VertexElementType* type);

SLX_API s_bool SLX_CALLCONV SLX_ReplaceInputBuffer(void* input, int32_t index, void* buffer, int32_t offset, int32_t stride);

SLX_API void* SLX_CALLCONV SLX_CreateVertexBuffer(int32_t size);
SLX_API s_bool SLX_CALLCONV SLX_DeleteVertexBuffer(void* buffer);
SLX_API s_bool SLX_CALLCONV SLX_SetVertexBufferData(void* buffer, void* data, int32_t offset, int32_t dataSize);

SLX_API void* SLX_CALLCONV SLX_CreateIndexBuffer(int32_t size);
SLX_API s_bool SLX_CALLCONV SLX_DeleteIndexBuffer(void* buffer);
SLX_API s_bool SLX_CALLCONV SLX_SetIndexBufferData(void* buffer, void* data, int32_t offset, int32_t dataSize);

SLX_API s_bool SLX_CALLCONV SLX_DrawPrimitives(void* input, PrimitiveType primitiveType, int32_t verticesCount);
SLX_API s_bool SLX_CALLCONV SLX_DrawIndexedPrimitives(void* input, void* indexBuffer, PrimitiveType primitiveType, int32_t indicesCount);

SLX_API void* SLX_CALLCONV SLX_CreateTexture(int32_t width, int32_t height);
SLX_API s_bool SLX_CALLCONV SLX_DeleteTexture(void* texture);
SLX_API s_bool SLX_CALLCONV SLX_SetTextureData(void* texture, int32_t width, int32_t height, void* data, ImageFormat imageFormat);
SLX_API s_bool SLX_CALLCONV SLX_SetTextureFilter(void* texture, TextureFilterType min, TextureFilterType max);
SLX_API s_bool SLX_CALLCONV SLX_SetTextureWrap(void* texture, TextureWrapType wrap);
SLX_API s_bool SLX_CALLCONV SLX_SetTexture(int32_t index, void* texture);

SLX_API void* SLX_CALLCONV SLX_CreateShaderFromGlsl(const char* vertSource, const char* fragSource);
SLX_API s_bool SLX_CALLCONV SLX_DeleteShader(void* shader);
SLX_API s_bool SLX_CALLCONV SLX_SetShader(void* shader);

SLX_API int SLX_CALLCONV SLX_GetShaderParamLocation(void* shader, const char* nameUtf8);
SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamInt(void* shader, int32_t loc, int32_t value);
SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamFloat(void* shader, int32_t loc, float value);
SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamVec4(void* shader, int32_t loc, P_IN float* vec);
SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamMat4(void* shader, int32_t loc, P_IN float* mat);
SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamMat3x2(void* shader, int32_t loc, P_IN float* mat);

SLX_API void* SLX_CALLCONV SLX_CreateSampler(TextureFilterType filterType, TextureWrapType wrapType);
SLX_API s_bool SLX_CALLCONV SLX_DeleteSampler(void* sampler);
SLX_API s_bool SLX_CALLCONV SLX_SetSampler(int32_t index, void* sampler);

SLX_API void* SLX_CALLCONV SLX_CreateRenderTarget(void* texture);
SLX_API s_bool SLX_CALLCONV SLX_DeleteRenderTarget(void* renderTarget);
SLX_API s_bool SLX_CALLCONV SLX_SetRenderTarget(void* renderTarget);

#endif