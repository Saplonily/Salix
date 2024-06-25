#include "api_graphics.h"

#include <cstdio>
#include <assert.h>
#include <vector>

#include "common.h"
#include "error.h"

static error_code gl_error_to_error_code(GLenum glerr);

#ifdef SLX_DEBUG

static const char* gl_error_to_string(GLenum glerr)
{
    switch (glerr)
    {
    case GL_NO_ERROR: return "GL_NO_ERROR";
    case GL_INVALID_ENUM: return "GL_INVALID_ENUM";
    case GL_INVALID_VALUE: return "GL_INVALID_VALUE";
    case GL_INVALID_FRAMEBUFFER_OPERATION: return "GL_INVALID_FRAMEBUFFER_OPERATION";
    case GL_OUT_OF_MEMORY: return "GL_OUT_OF_MEMORY";
    default: return "UNKNOWN_ERROR";
    }
}

static void on_gl_error(const char* func, int line, GLenum err)
{
    printf("[glError/%s:%d] %s\n", func, line, gl_error_to_string(err));
}

#define SLX_FAIL_ON_GL_ERROR() { if (GLenum err = glGetError() != GL_NO_ERROR) { \
    on_gl_error(__FUNCTION__, __LINE__, err); \
    SLX_FAIL(gl_error_to_error_code(err)); \
}}

#define SLX_FAIL_ON_GL_ERROR_NULL() { if (GLenum err = glGetError() != GL_NO_ERROR) { \
    on_gl_error(__FUNCTION__, __LINE__, err); \
    SLX_FAIL_NULL(gl_error_to_error_code(err)); \
}}

#define SLX_FAIL_ON_GL_ERROR_GOTO(label) { if (GLenum err = glGetError() != GL_NO_ERROR) { \
    on_gl_error(__FUNCTION__, __LINE__, err); \
    SLX_FAIL_GOTO(gl_error_to_error_code(err), label); \
}}

#else

#define SLX_FAIL_ON_GL_ERROR() { if (GLenum err = glGetError() != GL_NO_ERROR) { SLX_FAIL(gl_error_to_error_code(err)); }}
#define SLX_FAIL_ON_GL_ERROR_NULL() { if (GLenum err = glGetError() != GL_NO_ERROR) { SLX_FAIL_NULL(gl_error_to_error_code(err)); }}
#define SLX_FAIL_ON_GL_ERROR_GOTO(label) { if (GLenum err = glGetError() != GL_NO_ERROR) { SLX_FAIL_GOTO(gl_error_to_error_code(err), label); }}

#endif // SLX_DEBUG

#define SLX_FAIL_MAPENUM_COND(enum) { if (enum == -1) SLX_FAIL(error_code::enum_mapping_failed); }
#define SLX_FAIL_MAPENUM_COND_GOTO(enum, label) { if (enum == -1) SLX_FAIL_GOTO(error_code::enum_mapping_failed, label); }

static error_code gl_error_to_error_code(GLenum glerr)
{
    switch (glerr)
    {
    case GL_NO_ERROR: return error_code::ok;
    case GL_INVALID_ENUM: return error_code::context_gl_invalid_enum;
    case GL_INVALID_VALUE: return error_code::context_gl_invalid_value;
    case GL_INVALID_FRAMEBUFFER_OPERATION: return error_code::context_gl_invalid_framebuffer_operation;
    case GL_OUT_OF_MEMORY: return error_code::context_gl_out_of_memory;
    default: return error_code::context_gl_unknown_error;
    }
}

static GLuint cur_vao = 0;
static GLuint cur_vbo = 0;
static GLuint cur_fbo = 0;
static GLuint cur_shader = 0;
static GLuint default_vbo = 0;

static s_bool ensure_vao(GLuint vao)
{
    if (cur_vao != vao)
    {
        glBindVertexArray(vao);
        SLX_FAIL_ON_GL_ERROR();
        cur_vao = vao;
    }
    return false;
}

static s_bool ensure_vbo(GLuint vbo)
{
    if (cur_vbo != vbo)
    {
        glBindBuffer(GL_ARRAY_BUFFER, vbo);
        SLX_FAIL_ON_GL_ERROR();
        cur_vbo = vbo;
    }
    return false;
}

static s_bool make_vao(VertexElementType* type, int32_t len, P_OUT GLuint* out_vao)
{
    assert(type != 0);
    assert(len > 0);
    assert(out_vao != 0);
    GLuint vao = 0;
    assert(cur_vbo != 0);
    glGenVertexArrays(1, &vao);
    glBindVertexArray(vao);
    cur_vao = vao;

    int vertexSize = 0;
    for (int i = 0; i < len; i++)
    {
        vertex_element_glinfo t = VertexElementType_get_glinfo(type[i]);
        SLX_FAIL_COND_GOTO(t.type == 0, error_code::enum_mapping_failed, err);
        vertexSize += t.componentSize * t.count;
    }

    s_byte* currentOffset = 0;
    for (int i = 0; i < len; i++)
    {
        vertex_element_glinfo t = VertexElementType_get_glinfo(type[i]);
        glVertexAttribPointer(i, t.count, t.type, GL_FALSE, vertexSize, (void*)currentOffset);
        SLX_FAIL_ON_GL_ERROR_GOTO(err);
        glEnableVertexAttribArray(i);
        SLX_FAIL_ON_GL_ERROR_GOTO(err);
        currentOffset += (size_t)(t.componentSize * t.count);
    }
    *out_vao = vao;
    return false;
err:
    if (vao)
    {
        glDeleteVertexArrays(1, &vao);
        glBindVertexArray(0);
        cur_vao = 0;
    }
    return true;
}

// TODO: move these to managed
void graphics_initialize()
{
    glGenBuffers(1, &default_vbo);
    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
}

SLX_API void SLX_CALLCONV SLX_Viewport(int32_t x, int32_t y, int32_t width, int32_t height)
{
    glViewport(x, y, width, height);
}

SLX_API void SLX_CALLCONV SLX_Clear(float r, float g, float b, float a)
{
    assert(r >= 0.0f && g >= 0.0f && b >= 0.0f && a >= 0.0f);
    glClearColor(r, g, b, a);
    glClear(GL_COLOR_BUFFER_BIT);
}

SLX_API void* SLX_CALLCONV SLX_RegisterVertexType(P_IN VertexElementType* type, int32_t len)
{
    assert(type != nullptr);
    assert(len > 0);
    VertexElementType* tptr = new VertexElementType[len];
    memcpy(tptr, type, len * sizeof(VertexElementType));
    vertex_type_handle* h = new vertex_type_handle();
    h->type_ptr = tptr;
    h->length = len;
    h->default_vao_id = 0;
    return h;
}

SLX_API s_bool SLX_CALLCONV SLX_DrawPrimitives(
    P_IN vertex_type_handle* vertex_type,
    PrimitiveType pt,
    void* data, int32_t data_size,
    int32_t vertices_to_draw
)
{
    assert(vertex_type != 0);
    assert(data != 0);
    assert(data_size >= 1);
    assert(vertices_to_draw >= 1);
    if (ensure_vbo(default_vbo)) return true;

    if (vertex_type->default_vao_id == 0)
        if (make_vao(vertex_type->type_ptr, vertex_type->length, &vertex_type->default_vao_id))
            return true;
    if (ensure_vao(vertex_type->default_vao_id)) return true;

    glBufferData(GL_ARRAY_BUFFER, data_size, data, GL_DYNAMIC_DRAW);
    SLX_FAIL_ON_GL_ERROR();
    glDrawArrays(PrimitiveType_get_glinfo(pt), 0, vertices_to_draw);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_CreateVertexBuffer(P_IN vertex_type_handle* vertex_type, s_bool use_ibo, P_OUT buffer_handle** out_handle)
{
    assert(vertex_type != 0);
    assert(out_handle != 0);
    GLuint id;
    glGenBuffers(1, &id);
    ensure_vbo(id);
    buffer_handle* h = new buffer_handle;
    h->vbo_id = id;

    if (make_vao(vertex_type->type_ptr, vertex_type->length, &h->vao_id))
    {
        glDeleteBuffers(1, &id);
        return true;
    }

    h->ibo_id = 0;
    if (use_ibo)
    {
        glGenBuffers(1, &h->ibo_id);
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, h->ibo_id);
        SLX_FAIL_ON_GL_ERROR();
    }
    *out_handle = h;
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_DeleteVertexBuffer(P_IN buffer_handle* buffer)
{
    assert(buffer != 0);
    glDeleteVertexArrays(1, &buffer->vao_id);
    SLX_FAIL_ON_GL_ERROR();
    glDeleteBuffers(1, &buffer->vbo_id);
    SLX_FAIL_ON_GL_ERROR();
    if (buffer->ibo_id)
    {
        glDeleteBuffers(1, &buffer->ibo_id);
        SLX_FAIL_ON_GL_ERROR();
    }
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetVertexBufferData(buffer_handle* buffer_handle, void* data, int32_t dataSize, VertexBufferDataUsage data_usage)
{
    if (ensure_vbo(buffer_handle->vbo_id)) return true;
    if (ensure_vao(buffer_handle->vao_id)) return true;
    GLenum usage = VertexBufferDataUsage_to_gl(data_usage);
    SLX_FAIL_MAPENUM_COND(usage);
    glBufferData(GL_ARRAY_BUFFER, dataSize, data, usage);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_DrawBufferPrimitives(buffer_handle* buffer_handle, PrimitiveType primitiveType, int32_t verticesCount)
{
    if (ensure_vbo(buffer_handle->vbo_id)) return true;
    if (ensure_vao(buffer_handle->vao_id)) return true;
    GLenum type = PrimitiveType_get_glinfo(primitiveType);
    SLX_FAIL_MAPENUM_COND(type);
    glDrawArrays(type, 0, verticesCount);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetIndexBufferData(buffer_handle* buffer_handle, void* data, int32_t dataSize, VertexBufferDataUsage data_usage)
{
    assert(buffer_handle->ibo_id != 0);
    if (ensure_vao(buffer_handle->vao_id)) return true;
    GLenum usage = VertexBufferDataUsage_to_gl(data_usage);
    SLX_FAIL_MAPENUM_COND(usage);
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, dataSize, data, usage);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_DrawIndexedBufferPrimitives(buffer_handle* buffer_handle, PrimitiveType primitiveType, int32_t verticesCount)
{
    if (ensure_vbo(buffer_handle->vbo_id)) return true;
    if (ensure_vao(buffer_handle->vao_id)) return true;
    GLenum type = PrimitiveType_get_glinfo(primitiveType);
    SLX_FAIL_MAPENUM_COND(type);
    glDrawElements(type, verticesCount, GL_UNSIGNED_SHORT, 0);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_CreateTexture(int32_t width, int32_t height, P_OUT void** out_texture)
{
    GLuint tex;
    glGenTextures(1, &tex);
    SLX_FAIL_ON_GL_ERROR_GOTO(failed);
    glBindTexture(GL_TEXTURE_2D, tex);
    SLX_FAIL_ON_GL_ERROR_GOTO(failed);
    *out_texture = (void*)(size_t)tex;
    return false;
failed:
    if (tex) glDeleteTextures(1, &tex);
    return true;
}

SLX_API s_bool SLX_CALLCONV SLX_SetTextureFilter(void* tex_handle, TextureFilterType min, TextureFilterType max)
{
    assert(tex_handle != 0);
    GLuint tex = (GLuint)(size_t)tex_handle;
    glBindTexture(GL_TEXTURE_2D, tex);
    SLX_FAIL_ON_GL_ERROR();
    GLenum typeMin = TextureFilterType_to_gl(min);
    GLenum typeMax = TextureFilterType_to_gl(max);
    SLX_FAIL_MAPENUM_COND(typeMin);
    SLX_FAIL_MAPENUM_COND(typeMax);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, typeMin);
    SLX_FAIL_ON_GL_ERROR();
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, typeMax);
    SLX_FAIL_ON_GL_ERROR();

    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetTextureWrap(void* tex_handle, TextureWrapType wrap)
{
    assert(tex_handle != 0);
    GLuint tex = (GLuint)(size_t)tex_handle;
    glBindTexture(GL_TEXTURE_2D, tex);
    SLX_FAIL_ON_GL_ERROR();
    GLenum type = TextureWrapType_to_gl(wrap);
    SLX_FAIL_MAPENUM_COND(type);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, type);
    SLX_FAIL_ON_GL_ERROR();
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, type);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

// TODO cur tex is mutable
// TODO support more other formats
SLX_API s_bool SLX_CALLCONV SLX_SetTextureData(void* tex_handle, int32_t width, int32_t height, void* data, ImageFormat imageFormat)
{
    assert(tex_handle != 0);
    GLuint tex = (GLuint)(size_t)tex_handle;
    glBindTexture(GL_TEXTURE_2D, tex);
    SLX_FAIL_ON_GL_ERROR();
    int lineWidth = (width * ImageFormat_get_size(imageFormat));
    int align = lineWidth % 8 == 0 ? 8 : lineWidth % 4 == 0 ? 4 : lineWidth % 2 == 0 ? 2 : 1;
    glPixelStorei(GL_UNPACK_ALIGNMENT, align);
    GLenum format = ImageFormat_to_gl(imageFormat);
    glTexImage2D(GL_TEXTURE_2D, 0, format, width, height, 0, format, GL_UNSIGNED_BYTE, data);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

// TODO apply states?

SLX_API s_bool SLX_CALLCONV SLX_DeleteTexture(void* tex_handle)
{
    assert(tex_handle != 0);
    GLuint tex = (GLuint)(size_t)tex_handle;
    glDeleteTextures(1, &tex);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetTexture(int32_t index, void* tex_handle)
{
    assert(tex_handle != 0);
    glActiveTexture(GL_TEXTURE0 + index);
    SLX_FAIL_ON_GL_ERROR();
    glBindTexture(GL_TEXTURE_2D, (GLuint)(size_t)tex_handle);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_CreateShaderFromGlsl(const char* vert_source, const char* frag_source, P_OUT void** out_program)
{
    GLuint vsh = glCreateShader(GL_VERTEX_SHADER);
    GLuint fsh = glCreateShader(GL_FRAGMENT_SHADER);
    glShaderSource(vsh, 1, &vert_source, nullptr);
    glShaderSource(fsh, 1, &frag_source, nullptr);
    // TODO report the compile result
    glCompileShader(vsh);
    glCompileShader(fsh);
    GLuint prog = glCreateProgram();
    glAttachShader(prog, vsh);
    glAttachShader(prog, fsh);
    glLinkProgram(prog);
    glDeleteShader(vsh);
    glDeleteShader(fsh);
    SLX_FAIL_ON_GL_ERROR();
    *out_program = (void*)(size_t)prog;
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_DeleteShader(void* shader_handle)
{
    assert(shader_handle != 0);
    GLuint prog = (GLuint)(size_t)shader_handle;
    glDeleteProgram(prog);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetShader(void* shader_handle)
{
    assert(shader_handle != 0);
    GLuint prog = (GLuint)(size_t)shader_handle;
    if (cur_shader != prog)
    {
        glUseProgram(prog);
        SLX_FAIL_ON_GL_ERROR();
        cur_shader = prog;
    }
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_CreateSampler(TextureFilterType filter_type, TextureWrapType wrap_type, P_OUT void** out_sampler)
{
    GLuint sampler = 0;
    glGenSamplers(1, &sampler);
    SLX_FAIL_ON_GL_ERROR_GOTO(failed);

    GLenum filter = TextureFilterType_to_gl(filter_type);
    SLX_FAIL_MAPENUM_COND_GOTO(filter, failed);
    glSamplerParameteri(sampler, GL_TEXTURE_MIN_FILTER, filter);
    SLX_FAIL_ON_GL_ERROR_GOTO(failed);
    glSamplerParameteri(sampler, GL_TEXTURE_MAG_FILTER, filter);
    SLX_FAIL_ON_GL_ERROR_GOTO(failed);

    GLenum wrap = TextureWrapType_to_gl(wrap_type);
    SLX_FAIL_MAPENUM_COND_GOTO(wrap, failed);
    glSamplerParameteri(sampler, GL_TEXTURE_WRAP_S, wrap);
    SLX_FAIL_ON_GL_ERROR_GOTO(failed);
    glSamplerParameteri(sampler, GL_TEXTURE_WRAP_T, wrap);
    SLX_FAIL_ON_GL_ERROR_GOTO(failed);

    *out_sampler = (void*)(size_t)sampler;
    return false;
failed:
    if (sampler) glDeleteSamplers(1, &sampler);
    return true;
}

SLX_API s_bool SLX_CALLCONV SLX_SetSampler(int32_t index, void* sampler_handle)
{
    assert(sampler_handle != 0);
    glBindSampler(index, (GLuint)(size_t)sampler_handle);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_DeleteSampler(void* sampler_handle)
{
    assert(sampler_handle != 0);
    GLuint sampler = (GLuint)(size_t)sampler_handle;
    glDeleteSamplers(1, &sampler);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

#pragma region uniform

SLX_API int SLX_CALLCONV SLX_GetShaderParamLocation(void* shaderHandle, const char* nameUtf8)
{
    return glGetUniformLocation((GLuint)(size_t)shaderHandle, nameUtf8);
}

SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamInt(int32_t loc, int32_t value)
{
    glUniform1i(loc, value);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamFloat(int32_t loc, float value)
{
    glUniform1f(loc, value);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamVec4(int32_t loc, float* vec)
{
    glUniform4fv(loc, 1, vec);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamMat4(int32_t loc, float* mat)
{
    glUniformMatrix4fv(loc, 1, true, mat);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_SetShaderParamMat3x2(int32_t loc, float* mat)
{
    glUniformMatrix3x2fv(loc, 1, false, mat);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}

#pragma endregion

SLX_API s_bool SLX_CALLCONV SLX_CreateRenderTarget(void* tex_handle, P_OUT void** out_rt)
{
    assert(tex_handle != 0);

    error_code error_code = error_code::ok;

    unsigned int fbo;
    glGenFramebuffers(1, &fbo);

    glBindFramebuffer(GL_FRAMEBUFFER, fbo);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, (GLuint)(size_t)tex_handle, 0);

    SLX_FAIL_ON_GL_ERROR_GOTO(failed);
    glBindFramebuffer(GL_FRAMEBUFFER, cur_fbo);

    if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
    {
        error_code = error_code::gl_framebuffer_not_complete;
        goto failed;
    }

    *out_rt = (void*)(size_t)fbo;
    return false;
failed:
    if (fbo)
        glDeleteFramebuffers(1, &fbo);
    glBindFramebuffer(GL_FRAMEBUFFER, cur_fbo);
    return true;
}

SLX_API s_bool SLX_CALLCONV SLX_SetRenderTarget(void* fbo_handle)
{
    GLuint fbo = (GLuint)(size_t)fbo_handle;
    if (cur_fbo != fbo)
    {
        // no need to check wheather it's equals to 0
        glBindFramebuffer(GL_FRAMEBUFFER, fbo);
        SLX_FAIL_ON_GL_ERROR();
        cur_fbo = fbo;
    }
    return false;
}

SLX_API s_bool SLX_CALLCONV SLX_DeleteRenderTarget(void* fbo_handle)
{
    GLuint fbo = (GLuint)(size_t)fbo_handle;
    assert(fbo != 0);
    glDeleteFramebuffers(1, &fbo);
    SLX_FAIL_ON_GL_ERROR();
    return false;
}