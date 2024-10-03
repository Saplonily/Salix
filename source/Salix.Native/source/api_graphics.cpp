#include "api_graphics.h"

#include <cstdio>
#include <assert.h>

#include "memory.h"
#include "common.h"
#include "error.h"

#ifdef SLX_DEBUG

static const char* glErrorToString(GLenum glerr)
{
    switch (glerr)
    {
    case GL_NO_ERROR: return "GL_NO_ERROR";
    case GL_INVALID_ENUM: return "GL_INVALID_ENUM";
    case GL_INVALID_VALUE: return "GL_INVALID_VALUE";
    case GL_INVALID_FRAMEBUFFER_OPERATION: return "GL_INVALID_FRAMEBUFFER_OPERATION";
    case GL_OUT_OF_MEMORY: return "GL_OUT_OF_MEMORY";
    default: return "GL_UNKNOWN_ERROR";
    }
}

static void onGLError(const char* func, int line, GLenum err)
{
    printf("[glError/%s:%d] %s\n", func, line, glErrorToString(err));
}

#define SLX_FAIL_ON_GL_ERROR() { GLenum err; if ((err = glGetError()) != GL_NO_ERROR) { \
    onGLError(__FUNCTION__, __LINE__, err); \
    SLX_FAIL(slxMapGLError(err)); \
}}

#define SLX_FAIL_ON_GL_ERROR_NULL() { GLenum err; if ((err = glGetError()) != GL_NO_ERROR) { \
    onGLError(__FUNCTION__, __LINE__, err); \
    SLX_FAIL_NULL(slxMapGLError(err)); \
}}

#define SLX_FAIL_ON_GL_ERROR_RET(ret) { GLenum err; if ((err = glGetError()) != GL_NO_ERROR) { \
    onGLError(__FUNCTION__, __LINE__, err); \
    SLX_FAIL_RET(slxMapGLError(err), ret); \
}}

#define SLX_FAIL_ON_GL_ERROR_GOTO(label) { GLenum err; if ((err = glGetError()) != GL_NO_ERROR) { \
    onGLError(__FUNCTION__, __LINE__, err); \
    SLX_FAIL_GOTO(slxMapGLError(err), label); \
}}

#else

#define SLX_FAIL_ON_GL_ERROR() { GLenum err; if ((err = glGetError()) != GL_NO_ERROR) { SLX_FAIL(slxMapGLError(err)); }}
#define SLX_FAIL_ON_GL_ERROR_NULL() { GLenum err; if ((err = glGetError()) != GL_NO_ERROR) { SLX_FAIL_NULL(slxMapGLError(err)); }}
#define SLX_FAIL_ON_GL_ERROR_RET(ret) { GLenum err; if ((err = glGetError()) != GL_NO_ERROR) { SLX_FAIL_RET(slxMapGLError(err), ret); }}
#define SLX_FAIL_ON_GL_ERROR_GOTO(label) { GLenum err; if ((err = glGetError()) != GL_NO_ERROR) { SLX_FAIL_GOTO(slxMapGLError(err), label); }}

#endif // SLX_DEBUG

#define pack(glId) ((void*)(size_t)glId)
#define unpack(ptr) ((GLuint)(size_t)ptr)

#pragma region mapping

GLsizei slxVertexElementTypeGetSize(VertexElementType type)
{
    switch (type)
    {
    case VertexElementType::Color: return sizeof(float) * 4;
    case VertexElementType::Single: return sizeof(float) * 1;
    case VertexElementType::Vector2: return sizeof(float) * 2;
    case VertexElementType::Vector3: return sizeof(float) * 3;
    case VertexElementType::Vector4: return sizeof(float) * 4;
    }
    SLX_FAIL_MAP_ENUM();
}

VertexElementTypeGLSize slxVertexElementTypeGetGLSize(VertexElementType type)
{
    switch (type)
    {
    case VertexElementType::Color: return VertexElementTypeGLSize{ 4, GL_FLOAT, 4 * sizeof(float) };
    case VertexElementType::Single: return VertexElementTypeGLSize{ 1, GL_FLOAT, 1 * sizeof(float) };
    case VertexElementType::Vector2: return VertexElementTypeGLSize{ 2, GL_FLOAT, 2 * sizeof(float) };
    case VertexElementType::Vector3: return VertexElementTypeGLSize{ 3, GL_FLOAT, 3 * sizeof(float) };
    case VertexElementType::Vector4: return VertexElementTypeGLSize{ 4, GL_FLOAT, 4 * sizeof(float) };
    }
    SLX_FAIL_MAP_ENUM_RET((VertexElementTypeGLSize{ -1, (GLenum)-1, -1 }));
}

GLenum slxMapPrimitiveType(PrimitiveType type)
{
    switch (type)
    {
    case PrimitiveType::LineList: return GL_LINES;
    case PrimitiveType::LineStrip: return GL_LINE_STRIP;
    case PrimitiveType::PointList: return GL_POINTS;
    case PrimitiveType::TriangleFan: return GL_TRIANGLE_FAN;
    case PrimitiveType::TriangleList: return GL_TRIANGLES;
    case PrimitiveType::TriangleStrip: return GL_TRIANGLE_STRIP;
    }
    SLX_FAIL_MAP_ENUM();
}

GLenum slxMapImageFormat(ImageFormat format)
{
    switch (format)
    {
    case ImageFormat::R8: return GL_RED;
    case ImageFormat::Rg16: return GL_RG;
    case ImageFormat::Rgb24: return GL_RGB;
    case ImageFormat::Rgba32: return GL_RGBA;
    }
    SLX_FAIL_MAP_ENUM();
}

int slxImageFormatGetSize(ImageFormat format)
{
    switch (format)
    {
    case ImageFormat::R8: return 1;
    case ImageFormat::Rg16: return 2;
    case ImageFormat::Rgb24: return 3;
    case ImageFormat::Rgba32: return 4;
    }
    SLX_FAIL_MAP_ENUM();
}

GLenum slxMapTextureFilterType(TextureFilterType type)
{
    switch (type)
    {
    case TextureFilterType::Linear: return GL_LINEAR;
    case TextureFilterType::Nearest: return GL_NEAREST;
    case TextureFilterType::LinearMipmapLinear: return GL_LINEAR_MIPMAP_LINEAR;
    case TextureFilterType::LinearMipmapNearest: return GL_LINEAR_MIPMAP_NEAREST;
    case TextureFilterType::NearestMipmapLinear: return GL_NEAREST_MIPMAP_LINEAR;
    case TextureFilterType::NearestMipmapNearest: return GL_NEAREST_MIPMAP_NEAREST;
    }
    SLX_FAIL_MAP_ENUM();
}

GLenum slxMapTextureWrapType(TextureWrapType type)
{
    switch (type)
    {
    case TextureWrapType::ClampToEdge: return GL_CLAMP_TO_EDGE;
    case TextureWrapType::Repeat: return GL_REPEAT;
    case TextureWrapType::MirroredRepeat: return GL_MIRRORED_REPEAT;
    }
    SLX_FAIL_MAP_ENUM();
}

ErrorCode slxMapGLError(GLenum glerr)
{
    switch (glerr)
    {
    case GL_NO_ERROR: return ErrorCode::OK;
    case GL_INVALID_ENUM: return ErrorCode::ContextGLInvalidEnum;
    case GL_INVALID_VALUE: return ErrorCode::ContextGLInvalidValue;
    case GL_INVALID_FRAMEBUFFER_OPERATION: return ErrorCode::ContextGLInvalidFramebufferOperation;
    case GL_OUT_OF_MEMORY: return ErrorCode::ContextGLOutOfMemory;
    default: return ErrorCode::ContextGLUnknownError;
    }
}

#pragma endregion

// TODO: move these to managed
void graphics_initialize()
{
    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
}

SLX_API s_bool SLX_CALLCONV SLX_Viewport(int32_t x, int32_t y, int32_t width, int32_t height)
{
    assert(x >= 0 && y >= 0 && width >= 1 && height >= 1);

    glViewport(x, y, width, height);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_Clear(float r, float g, float b, float a)
{
    assert(r >= 0.0f && g >= 0.0f && b >= 0.0f && a >= 0.0f);

    glClearColor(r, g, b, a);
    glClear(GL_COLOR_BUFFER_BIT);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API void* SLX_CALLCONV SLX_CreateVertexBuffersInput(int32_t count, VertexBufferBinding* bindings)
{
    GLuint vao = 0;
    glCreateVertexArrays(1, &vao);
    SLX_FAIL_ON_GL_ERROR_GOTO(err);

    for (int32_t slot = 0; slot < count; slot++)
    {
        VertexBufferBinding binding = bindings[slot];

        int32_t stride = 0;
        for (int32_t i = 0; i < binding.elementsCount; i++)
        {
            int32_t size = slxVertexElementTypeGetSize(binding.type[i]);
            if (size == -1) return nullptr;
            stride += size;
        }

        int32_t offset = 0;
        for (int32_t i = 0; i < binding.elementsCount; i++)
        {
            VertexElementTypeGLSize size = slxVertexElementTypeGetGLSize(binding.type[i]);
            if (size.count == -1) return nullptr;
            glEnableVertexArrayAttrib(vao, i);
            glVertexArrayAttribBinding(vao, i, slot);
            glVertexArrayVertexBuffer(vao, slot, unpack(binding.buffer), binding.offset, stride);
            glVertexArrayAttribFormat(vao, i, size.count, size.type, GL_FALSE, offset);
            glVertexArrayBindingDivisor(vao, i, binding.instanceFrequency);
            SLX_FAIL_ON_GL_ERROR_GOTO(err);
            offset += size.size;
        }
    }
    return pack(vao);
err:
    glDeleteBuffers(1, &vao);
    return nullptr;
}

SLX_API void* SLX_CALLCONV SLX_CreateSingleVertexBufferInput(int32_t elementsCount, VertexElementType* type)
{
    GLuint vao = 0;
    glCreateVertexArrays(1, &vao);
    SLX_FAIL_ON_GL_ERROR_GOTO(err);

    int32_t stride = 0;
    for (int32_t i = 0; i < elementsCount; i++)
    {
        int32_t size = slxVertexElementTypeGetSize(type[i]);
        if (size == -1) return nullptr;
        stride += size;
    }

    int32_t offset = 0;
    for (int32_t i = 0; i < elementsCount; i++)
    {
        VertexElementTypeGLSize size = slxVertexElementTypeGetGLSize(type[i]);
        if (size.count == -1) return nullptr;
        glEnableVertexArrayAttrib(vao, i);
        glVertexArrayAttribBinding(vao, i, 0);
        glVertexArrayAttribFormat(vao, i, size.count, size.type, GL_FALSE, offset);
        SLX_FAIL_ON_GL_ERROR_GOTO(err);
        offset += size.size;
    }

    return pack(vao);
err:
    glDeleteBuffers(1, &vao);
    return nullptr;
}

SLX_API s_bool SLX_CALLCONV SLX_ReplaceInputBuffer(void* input, int32_t index, void* buffer, int32_t offset, int32_t stride)
{
    glVertexArrayVertexBuffer(unpack(input), index, unpack(buffer), offset, stride);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

static void* createBuffer(GLenum target, int32_t size)
{
    assert(size > 0);
    GLuint buffer;
    glCreateBuffers(1, &buffer);
    glNamedBufferStorage(buffer, size, nullptr, GL_DYNAMIC_STORAGE_BIT);
    SLX_FAIL_ON_GL_ERROR_GOTO(err);
    return pack(buffer);

err:
    if (buffer) glDeleteBuffers(1, &buffer);
    return nullptr;
}

SLX_API void* SLX_CALLCONV SLX_CreateVertexBuffer(int32_t size)
{
    return createBuffer(GL_ARRAY_BUFFER, size);
}

SLX_API void* SLX_CALLCONV SLX_CreateIndexBuffer(int32_t size)
{
    return createBuffer(GL_ELEMENT_ARRAY_BUFFER, size);
}

static s_bool deleteBuffer(GLuint buffer)
{
    glDeleteBuffers(1, &buffer);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_DeleteVertexBuffer(void* buffer)
{
    assert(buffer != nullptr);
    return deleteBuffer(unpack(buffer));
}

SLX_API s_bool SLX_CALLCONV SLX_DeleteIndexBuffer(void* buffer)
{
    assert(buffer != nullptr);
    return deleteBuffer(unpack(buffer));
}

static s_bool setBufferData(GLenum buffer, void* data, int32_t offset, int32_t dataSize)
{
    assert(buffer != 0);
    assert(data != nullptr);
    assert(dataSize > 0);

    glNamedBufferSubData(buffer, offset, dataSize, data);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetVertexBufferData(void* buffer, void* data, int32_t offset, int32_t dataSize)
{
    return setBufferData(unpack(buffer), data, offset, dataSize);
}

SLX_API s_bool SLX_CALLCONV SLX_SetIndexBufferData(void* buffer, void* data, int32_t offset, int32_t dataSize)
{
    return setBufferData(unpack(buffer), data, offset, dataSize);
}

SLX_API s_bool SLX_CALLCONV SLX_DrawPrimitives(void* input, PrimitiveType primitiveType, int32_t verticesCount)
{
    assert(input != nullptr);
    assert(verticesCount >= 1);

    glBindVertexArray(unpack(input));

    GLenum type = slxMapPrimitiveType(primitiveType);
    if (type == -1) return true;
    glDrawArrays(type, 0, verticesCount);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_DrawIndexedPrimitives(void* input, void* indexBuffer, PrimitiveType primitiveType, int32_t indicesCount)
{
    assert(input != nullptr);
    assert(indicesCount >= 1);

    glBindVertexArray(unpack(input));
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, unpack(indexBuffer));

    GLenum type = slxMapPrimitiveType(primitiveType);
    if (type == -1) return true;
    glDrawElements(type, indicesCount, GL_UNSIGNED_SHORT, nullptr);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API void* SLX_CALLCONV SLX_CreateTexture(int32_t width, int32_t height)
{
    assert(width >= 1);
    assert(height >= 1);

    GLuint tex;
    glCreateTextures(GL_TEXTURE_2D, 1, &tex);
    glTextureStorage2D(tex, 1, GL_RGBA8, width, height);

    SLX_FAIL_ON_GL_ERROR_GOTO(failed);
    return pack(tex);

failed:
    if (tex) glDeleteTextures(1, &tex);
    return nullptr;
}

SLX_API s_bool SLX_CALLCONV SLX_SetTextureFilter(void* texture, TextureFilterType min, TextureFilterType max)
{
    assert(texture != nullptr);

    GLuint tex = unpack(texture);
    GLenum typeMin = slxMapTextureFilterType(min);
    GLenum typeMax = slxMapTextureFilterType(max);
    if (typeMin == -1 || typeMax == -1) return true;
    glTextureParameteri(tex, GL_TEXTURE_MIN_FILTER, typeMin);
    SLX_FAIL_ON_GL_ERROR();
    glTextureParameteri(tex, GL_TEXTURE_MAG_FILTER, typeMax);
    SLX_FAIL_ON_GL_ERROR();

    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetTextureWrap(void* texture, TextureWrapType wrap)
{
    assert(texture != nullptr);

    GLuint tex = unpack(texture);
    GLenum type = slxMapTextureWrapType(wrap);
    if (type == -1) return true;
    glTextureParameteri(tex, GL_TEXTURE_WRAP_S, type);
    SLX_FAIL_ON_GL_ERROR();
    glTextureParameteri(tex, GL_TEXTURE_WRAP_T, type);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

// TODO support more other formats
SLX_API s_bool SLX_CALLCONV SLX_SetTextureData(void* texture, int32_t width, int32_t height, void* data, ImageFormat imageFormat)
{
    assert(texture != nullptr);
    assert(width > 0);
    assert(height > 0);
    assert(data != nullptr);

    GLuint tex = unpack(texture);
    int lineWidth = (width * slxImageFormatGetSize(imageFormat));
    int align = lineWidth % 8 == 0 ? 8 : lineWidth % 4 == 0 ? 4 : lineWidth % 2 == 0 ? 2 : 1;
    glPixelStorei(GL_UNPACK_ALIGNMENT, align);
    GLenum format = slxMapImageFormat(imageFormat);
    glTextureSubImage2D(tex, 1, 0, 0, width, height, format, GL_UNSIGNED_BYTE, data);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_DeleteTexture(void* texture)
{
    assert(texture != nullptr);

    GLuint tex = unpack(texture);
    glDeleteTextures(1, &tex);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetTexture(int32_t index, void* texture)
{
    assert(index >= 0);
    assert(texture != nullptr);

    GLuint tex = unpack(texture);
    glBindTextureUnit(index, tex);
    SLX_FAIL_ON_GL_ERROR();

    return false;
}

SLX_API void* SLX_CALLCONV SLX_CreateShaderFromGlsl(const char* vertSource, const char* fragSource)
{
    assert(vertSource != nullptr);
    assert(fragSource != nullptr);

    GLuint vsh = glCreateShader(GL_VERTEX_SHADER);
    GLuint fsh = glCreateShader(GL_FRAGMENT_SHADER);
    glShaderSource(vsh, 1, &vertSource, nullptr);
    glShaderSource(fsh, 1, &fragSource, nullptr);
    // TODO report the compile result
    glCompileShader(vsh);
    glCompileShader(fsh);
    GLuint prog = glCreateProgram();
    glAttachShader(prog, vsh);
    glAttachShader(prog, fsh);
    glLinkProgram(prog);
    glDeleteShader(vsh);
    glDeleteShader(fsh);
    SLX_FAIL_ON_GL_ERROR_NULL();
    return pack(prog);
}

SLX_API s_bool SLX_CALLCONV SLX_DeleteShader(void* shader)
{
    assert(shader != 0);

    GLuint prog = unpack(shader);
    glDeleteProgram(prog);
    SLX_FAIL_ON_GL_ERROR();

    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetShader(void* shader)
{
    assert(shader != 0);

    GLuint prog = unpack(shader);
    glUseProgram(prog);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API void* SLX_CALLCONV SLX_CreateSampler(TextureFilterType filterType, TextureWrapType wrapType)
{
    GLuint sampler = 0;
    glCreateSamplers(1, &sampler);
    SLX_FAIL_ON_GL_ERROR_GOTO(failed);

    GLenum filter = slxMapTextureFilterType(filterType);
    if (filter == -1) goto failed;
    glSamplerParameteri(sampler, GL_TEXTURE_MIN_FILTER, filter);
    SLX_FAIL_ON_GL_ERROR_GOTO(failed);
    glSamplerParameteri(sampler, GL_TEXTURE_MAG_FILTER, filter);
    SLX_FAIL_ON_GL_ERROR_GOTO(failed);

    GLenum wrap = slxMapTextureWrapType(wrapType);
    if (wrap == -1) goto failed;
    glSamplerParameteri(sampler, GL_TEXTURE_WRAP_S, wrap);
    SLX_FAIL_ON_GL_ERROR_GOTO(failed);
    glSamplerParameteri(sampler, GL_TEXTURE_WRAP_T, wrap);
    SLX_FAIL_ON_GL_ERROR_GOTO(failed);

    return pack(sampler);
failed:
    if (sampler) glDeleteSamplers(1, &sampler);
    return nullptr;
}

SLX_API s_bool SLX_CALLCONV SLX_SetSampler(int32_t index, void* sampler)
{
    assert(sampler != nullptr);

    glBindSampler(index, unpack(sampler));
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_DeleteSampler(void* sampler)
{
    assert(sampler != nullptr);

    GLuint spl = unpack(sampler);
    glDeleteSamplers(1, &spl);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

#pragma region uniform

SLX_API int SLX_CALLCONV SLX_GetShaderParamLocation(void* shader, const char* nameUtf8)
{
    assert(shader != nullptr);

    int ret = glGetUniformLocation(unpack(shader), nameUtf8);
    SLX_FAIL_ON_GL_ERROR_RET(-2);
    return ret;
}

SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamInt(void* shader, int32_t loc, int32_t value)
{
    assert(loc >= 0);

    GLuint prog = unpack(shader);
    glProgramUniform1i(prog, loc, value);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamFloat(void* shader, int32_t loc, float value)
{
    assert(loc >= 0);

    GLuint prog = unpack(shader);
    glProgramUniform1f(prog, loc, value);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamVec4(void* shader, int32_t loc, P_IN float* vec)
{
    assert(loc >= 0);

    GLuint prog = unpack(shader);
    glProgramUniform4fv(prog, loc, 1, vec);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamMat4(void* shader, int32_t loc, P_IN float* mat)
{
    assert(loc >= 0);

    GLuint prog = unpack(shader);
    glProgramUniformMatrix4fv(prog, loc, 1, true, mat);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamMat3x2(void* shader, int32_t loc, P_IN float* mat)
{
    assert(loc >= 0);

    GLuint prog = unpack(shader);
    glProgramUniformMatrix3x2fv(prog, loc, 1, false, mat);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

#pragma endregion

SLX_API void* SLX_CALLCONV SLX_CreateRenderTarget(void* texture)
{
    assert(texture != nullptr);

    ErrorCode error_code = ErrorCode::OK;

    unsigned int fbo;
    glCreateFramebuffers(1, &fbo);
    glNamedFramebufferTexture(fbo, GL_COLOR_ATTACHMENT0, unpack(texture), 0);

    SLX_FAIL_ON_GL_ERROR_GOTO(failed);

    if (glCheckNamedFramebufferStatus(fbo, GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
    {
        error_code = ErrorCode::ContextGLFramebufferNotComplete;
        goto failed;
    }

    return pack(fbo);
failed:
    if (fbo) glDeleteFramebuffers(1, &fbo);
    return nullptr;
}

SLX_API s_bool SLX_CALLCONV SLX_SetRenderTarget(void* fbo)
{
    assert(fbo != nullptr);

    GLuint glFbo = unpack(fbo);
    glBindFramebuffer(GL_FRAMEBUFFER, glFbo);
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_DeleteRenderTarget(void* fbo)
{
    assert(fbo != nullptr);

    GLuint glFbo = unpack(fbo);
    glDeleteFramebuffers(1, &glFbo);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}