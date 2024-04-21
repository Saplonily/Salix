#include <Dwmapi.h>
#include <cstdio>
#include <assert.h>
#include <vector>
#include <glad/glad.h>
#include <glad/glad_wgl.h>
#include "windowing.h"
#include "common.h"
#include "graphics_enums.h"

// see more at MsdgSwapBuffers
//#define MSDG_COMPATIBILITY_GL

struct vertex_type_handle
{
    VertexElementType* type_ptr;
    int length;
    // for the default 'DrawPrimitive' method
    GLuint default_vao_id;
};

struct buffer_handle
{
    GLuint vbo_id;
    GLuint vao_id;
    GLuint ibo_id;
};

// TODO better error checking (report to managed code)
#define GL_CHECK_ERROR {\
GLenum err = glGetError();\
if (err != 0)\
printf("[glGetError] [%s:%d] %s\n", __FUNCTION__, __LINE__, gl_get_error_msg(err));\
}\

static GLuint cur_vao = 0;
static GLuint cur_vbo = 0;
static GLuint cur_shd = 0;
static GLuint cur_fbo = 0;
static GLuint default_vbo = 0;
static void ensure_vao(GLuint vao);
static void ensure_vbo(GLuint vbo);
static GLuint make_vao(VertexElementType* type, int len);


EXPORT GraphicsBackend CALLCONV MsdgGetGraphicsBackend()
{
    return GraphicsBackend::Opengl33;
}

EXPORT void CALLCONV MsdgSwapBuffers(msd_window* win)
{
    // FIXME some screen recorders may break our program here
    // for example, OCam and Bandicam
    // but others like XBox Capture and OBS won't

    // Currently I've only found that this problem can be resolved by setting
    // WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB to the WGL_CONTEXT_PROFILE_MASK_ARB,
    // for now it can be enabled by uncommenting the MSDG_COMPATIBILITY_GL macro definition in the exports.h.
    // If you have a better solution, welcome your contributions!
    SwapBuffers(win->hdc);
}

EXPORT double CALLCONV MsdgGetVSyncFrameTime()
{
    HDC hdc = GetDC(NULL);
    int rate = GetDeviceCaps(hdc, VREFRESH);
    ReleaseDC(NULL, hdc);
    return 1.0 / rate;
}

EXPORT void CALLCONV MsdgViewport(int32_t x, int32_t y, int32_t width, int32_t height)
{
    glViewport(x, y, width, height);
    GL_CHECK_ERROR;
}

EXPORT void CALLCONV MsdgClear(float* color)
{
    glClearColor(color[0], color[1], color[2], color[3]);
    GL_CHECK_ERROR;
    glClear(GL_COLOR_BUFFER_BIT);
    GL_CHECK_ERROR;
}

EXPORT void CALLCONV MsdgSetVSyncEnabled(byte enable)
{
    wglSwapIntervalEXT(enable ? 1 : 0);
}

EXPORT void* CALLCONV MsdgRegisterVertexType(VertexElementType* type, int32_t len)
{
    VertexElementType* tptr = new VertexElementType[len];
    memcpy(tptr, type, len * sizeof(VertexElementType));
    vertex_type_handle* h = new vertex_type_handle;
    h->type_ptr = tptr;
    h->length = len;
    h->default_vao_id = 0;
    return h;
}

EXPORT void CALLCONV MsdgDrawPrimitives(vertex_type_handle* vertex_type, PrimitiveType pt, void* data, int32_t data_size, int32_t vertices_to_draw)
{
    ensure_vbo(default_vbo);
    if (vertex_type->default_vao_id == 0)
        vertex_type->default_vao_id = make_vao(vertex_type->type_ptr, vertex_type->length);
    ensure_vao(vertex_type->default_vao_id);

    glBufferData(GL_ARRAY_BUFFER, data_size, data, GL_DYNAMIC_DRAW);
    GL_CHECK_ERROR;
    glDrawArrays(PrimitiveType_get_glinfo(pt), 0, vertices_to_draw);
    GL_CHECK_ERROR;
}

EXPORT buffer_handle* CALLCONV MsdgCreateVertexBuffer(vertex_type_handle* vertex_type, byte use_ibo)
{
    GLuint id;
    glGenBuffers(1, &id);
    GL_CHECK_ERROR;
    ensure_vbo(id);
    buffer_handle* h = new buffer_handle;
    h->vbo_id = id;
    h->vao_id = make_vao(vertex_type->type_ptr, vertex_type->length);
    h->ibo_id = 0;
    if (use_ibo)
    {
        glGenBuffers(1, &h->ibo_id); GL_CHECK_ERROR;
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, h->ibo_id); GL_CHECK_ERROR;
    }
    return h;
}

EXPORT void CALLCONV MsdgDeleteVertexBuffer(buffer_handle* buffer)
{
    glDeleteVertexArrays(1, &buffer->vao_id);
    GL_CHECK_ERROR;
    glDeleteBuffers(1, &buffer->vbo_id);
    GL_CHECK_ERROR;
    if (buffer->ibo_id)
        glDeleteBuffers(1, &buffer->ibo_id);
}

EXPORT void CALLCONV MsdgSetVertexBufferData(buffer_handle* buffer_handle, void* data, int32_t dataSize, VertexBufferDataUsage data_usage)
{
    ensure_vbo(buffer_handle->vbo_id);
    ensure_vao(buffer_handle->vao_id);
    glBufferData(GL_ARRAY_BUFFER, dataSize, data, VertexBufferDataUsage_to_gl(data_usage));
    GL_CHECK_ERROR;
}

EXPORT void CALLCONV MsdgDrawBufferPrimitives(buffer_handle* buffer_handle, PrimitiveType primitiveType, int32_t verticesCount)
{
    ensure_vbo(buffer_handle->vbo_id);
    ensure_vao(buffer_handle->vao_id);
    glDrawArrays(PrimitiveType_get_glinfo(primitiveType), 0, verticesCount);
    GL_CHECK_ERROR;
}

EXPORT void CALLCONV MsdgSetIndexBufferData(buffer_handle* buffer_handle, void* data, int32_t dataSize, VertexBufferDataUsage data_usage)
{
    assert(buffer_handle->ibo_id != 0);
    ensure_vao(buffer_handle->vao_id);
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, dataSize, data, VertexBufferDataUsage_to_gl(data_usage));
    GL_CHECK_ERROR;
}

EXPORT void CALLCONV MsdgDrawIndexedBufferPrimitives(buffer_handle* buffer_handle, PrimitiveType primitiveType, int32_t verticesCount)
{
    ensure_vbo(buffer_handle->vbo_id);
    ensure_vao(buffer_handle->vao_id);
    glDrawElements(PrimitiveType_get_glinfo(primitiveType), verticesCount, GL_UNSIGNED_SHORT, 0);
    GL_CHECK_ERROR;
}

EXPORT void* CALLCONV MsdgCreateTexture(int32_t width, int32_t height)
{
    GLuint tex;
    glGenTextures(1, &tex); GL_CHECK_ERROR;
    glBindTexture(GL_TEXTURE_2D, tex); GL_CHECK_ERROR;
    return (void*)(size_t)tex;
}

EXPORT void CALLCONV MsdgSetTextureFilter(void* tex_handle, TextureFilterType min, TextureFilterType max)
{
    GLuint tex = (GLuint)(size_t)tex_handle;
    glBindTexture(GL_TEXTURE_2D, tex); GL_CHECK_ERROR;
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, TextureFilterType_to_gl(min)); GL_CHECK_ERROR;
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, TextureFilterType_to_gl(max)); GL_CHECK_ERROR;
}

EXPORT void CALLCONV MsdgSetTextureWrap(void* tex_handle, TextureWrapType wrap)
{
    GLuint tex = (GLuint)(size_t)tex_handle;
    glBindTexture(GL_TEXTURE_2D, tex); GL_CHECK_ERROR;
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, TextureWrapType_to_gl(wrap)); GL_CHECK_ERROR;
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, TextureWrapType_to_gl(wrap)); GL_CHECK_ERROR;
}

// TODO support more other formats
EXPORT void CALLCONV MsdgSetTextureData(void* tex_handle, int32_t width, int32_t height, void* data, ImageFormat imageFormat)
{
    GLuint tex = (GLuint)(size_t)tex_handle;
    glBindTexture(GL_TEXTURE_2D, tex);
    int lineWidth = (width * ImageFormat_get_size(imageFormat));
    int align = lineWidth % 8 == 0 ? 8 : lineWidth % 4 == 0 ? 4 : lineWidth % 2 == 0 ? 2 : 1;
    glPixelStorei(GL_UNPACK_ALIGNMENT, align); GL_CHECK_ERROR;
    GLenum format = ImageFormat_to_gl(imageFormat);
    glTexImage2D(GL_TEXTURE_2D, 0, format, width, height, 0, format, GL_UNSIGNED_BYTE, data); GL_CHECK_ERROR;
}

EXPORT void CALLCONV MsdgDeleteTexture(void* tex_handle)
{
    GLuint tex = (GLuint)(size_t)tex_handle;
    assert(tex != 0);
    glDeleteTextures(1, &tex); GL_CHECK_ERROR;
}

EXPORT void CALLCONV MsdgSetTexture(int32_t index, void* tex_handle)
{
    glActiveTexture(GL_TEXTURE0 + index); GL_CHECK_ERROR;
    glBindTexture(GL_TEXTURE_2D, (GLuint)(size_t)tex_handle); GL_CHECK_ERROR;

}

EXPORT void* CALLCONV MsdgCreateShaderFromGlsl(const char* vert_source, const char* frag_source)
{
    GLuint vsh = glCreateShader(GL_VERTEX_SHADER); GL_CHECK_ERROR;
    GLuint fsh = glCreateShader(GL_FRAGMENT_SHADER); GL_CHECK_ERROR;
    glShaderSource(vsh, 1, &vert_source, nullptr); GL_CHECK_ERROR;
    glShaderSource(fsh, 1, &frag_source, nullptr); GL_CHECK_ERROR;
    glCompileShader(vsh); GL_CHECK_ERROR;
    glCompileShader(fsh); GL_CHECK_ERROR;
    GLuint prog = glCreateProgram(); GL_CHECK_ERROR;
    glAttachShader(prog, vsh); GL_CHECK_ERROR;
    glAttachShader(prog, fsh); GL_CHECK_ERROR;
    glLinkProgram(prog); GL_CHECK_ERROR;
    glDeleteShader(vsh); GL_CHECK_ERROR;
    glDeleteShader(fsh); GL_CHECK_ERROR;
    return (void*)(size_t)prog;
}

EXPORT void CALLCONV MsdgDeleteShader(void* shader_handle)
{
    GLuint prog = (GLuint)(size_t)shader_handle;
    assert(prog != 0);
    glDeleteProgram(prog); GL_CHECK_ERROR;
}

EXPORT void CALLCONV MsdgSetShader(void* shader_handle)
{
    GLuint prog = (GLuint)(size_t)shader_handle;
    if (cur_shd != prog)
    {
        glUseProgram(prog); GL_CHECK_ERROR;
        cur_shd = prog;
    }
}

#pragma region uniform
EXPORT int CALLCONV MsdgGetShaderParamLocation(void* shaderHandle, const char* nameUtf8)
{
    return glGetUniformLocation((GLuint)(size_t)shaderHandle, nameUtf8); GL_CHECK_ERROR;
}
EXPORT void CALLCONV MsdgSetShaderParamInt(int32_t loc, int32_t value)
{
    glUniform1i(loc, value); GL_CHECK_ERROR;
}
EXPORT void CALLCONV MsdgSetShaderParamFloat(int32_t loc, float value)
{
    glUniform1f(loc, value); GL_CHECK_ERROR;
}
EXPORT void CALLCONV MsdgSetShaderParamVec4(int32_t loc, float* vec)
{
    glUniform4fv(loc, 1, vec); GL_CHECK_ERROR;
}
EXPORT void CALLCONV MsdgSetShaderParamMat4(int32_t loc, float* mat)
{
    glUniformMatrix4fv(loc, 1, true, mat); GL_CHECK_ERROR;
}
EXPORT void CALLCONV MsdgSetShaderParamMat3x2(int32_t loc, float* mat)
{
    glUniformMatrix3x2fv(loc, 1, false, mat); GL_CHECK_ERROR;
}
#pragma endregion

EXPORT void* CALLCONV MsdgCreateRenderTarget(void* tex_handle)
{
    assert(tex_handle != 0);
    unsigned int fbo;
    glGenFramebuffers(1, &fbo); GL_CHECK_ERROR;

    glBindFramebuffer(GL_FRAMEBUFFER, fbo); GL_CHECK_ERROR;
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, (GLuint)(size_t)tex_handle, 0); GL_CHECK_ERROR;
    glBindFramebuffer(GL_FRAMEBUFFER, cur_fbo); GL_CHECK_ERROR;

    // TODO error handling
    assert(glCheckFramebufferStatus(GL_FRAMEBUFFER) == GL_FRAMEBUFFER_COMPLETE); GL_CHECK_ERROR;
    return (void*)(size_t)fbo;
}

EXPORT void CALLCONV MsdgSetRenderTarget(void* fbo_handle)
{
    GLuint fbo = (GLuint)(size_t)fbo_handle;
    if (cur_fbo != fbo)
    {
        // no need to check wheather it's equals to 0
        glBindFramebuffer(GL_FRAMEBUFFER, fbo); GL_CHECK_ERROR;
        cur_fbo = fbo;
    }
}

EXPORT void CALLCONV MsdgDeleteRenderTarget(void* fbo_handle)
{
    GLuint fbo = (GLuint)(size_t)fbo_handle;
    assert(fbo != 0);
    glDeleteFramebuffers(1, &fbo);
}

static void ensure_vao(GLuint vao)
{
    if (cur_vao != vao)
    {
        glBindVertexArray(vao); GL_CHECK_ERROR;
        cur_vao = vao;
    }
}

static void ensure_vbo(GLuint vbo)
{
    if (cur_vbo != vbo)
    {
        assert(vbo != 0);
        glBindBuffer(GL_ARRAY_BUFFER, vbo); GL_CHECK_ERROR;
        cur_vbo = vbo;
    }
}

static GLuint make_vao(VertexElementType* type, int32_t len)
{
    assert(type != nullptr && len >= 1);
    assert(cur_vbo != 0);
    GLuint vao;
    glGenVertexArrays(1, &vao); GL_CHECK_ERROR;
    glBindVertexArray(vao); GL_CHECK_ERROR;
    cur_vao = vao;
    int singleVertexSize = 0;
    for (int i = 0; i < len; i++)
    {
        vertex_element_glinfo t = VertexElementType_get_glinfo(type[i]);
        singleVertexSize += t.componentSize * t.count;
    }
    byte* currentOffset = 0;

    for (int i = 0; i < len; i++)
    {
        vertex_element_glinfo t = VertexElementType_get_glinfo(type[i]);
        glVertexAttribPointer(i, t.count, t.type, GL_FALSE, singleVertexSize, (void*)currentOffset); GL_CHECK_ERROR;
        glEnableVertexAttribArray(i); GL_CHECK_ERROR;
        currentOffset += (size_t)(t.componentSize * t.count);
    }
    return vao;
}

void render_context_graphics_init()
{
    glGenBuffers(1, &default_vbo); GL_CHECK_ERROR;
    glEnable(GL_BLEND); GL_CHECK_ERROR;
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA); GL_CHECK_ERROR;
}