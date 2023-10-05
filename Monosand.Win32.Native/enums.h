#pragma once
#ifndef H_ENUMS
#define H_ENUMS
#include "pch.h"

// ../Monosand/Graphics/VertexElementType.cs
enum class VertexElementType
{
    Single,
    Color,
    Vector2,
    Vector3,
    Vector4,
};

// ../Monosand/Graphics/PrimitiveType.cs
enum class PrimitiveType
{
    TriangleList,
    TriangleStrip,
    LineList,
    LineStrip,
    PointList
};

// ../Monosand/Graphics/VertexBufferDataUsage.cs
enum class VertexBufferDataUsage
{
    StaticDraw,
    DynamicDraw,
    StreamDraw
};

// ../Monosand/Graphics/GraphicsBackend.cs
enum class GraphicsBackend
{
    Unknown,
    Opengl33,
    DirectX11,
};

// ../Monosand/Graphics/ImageFormat.cs
enum class ImageFormat
{
    R8,
    Rg16,
    Rgb24,
    Rgba32
};

struct vertex_element_glinfo { int count; GLenum type; GLsizei componentSize; };

inline vertex_element_glinfo VertexElementType_get_glinfo(VertexElementType type)
{
    switch (type)
    {
    case VertexElementType::Single:
        return vertex_element_glinfo{ 1, GL_FLOAT, sizeof(float) };
    case VertexElementType::Color:
        return vertex_element_glinfo{ 4, GL_FLOAT, sizeof(float) };
    case VertexElementType::Vector2:
        return vertex_element_glinfo{ 2, GL_FLOAT, sizeof(float) };
    case VertexElementType::Vector3:
        return vertex_element_glinfo{ 3, GL_FLOAT, sizeof(float) };
    case VertexElementType::Vector4:
        return vertex_element_glinfo{ 4, GL_FLOAT, sizeof(float) };
    }
    assert(false);
    return vertex_element_glinfo{ -1 , NULL, -1 };
    // TODO: error handling
}

inline GLenum PrimitiveType_get_glinfo(PrimitiveType type)
{
    switch (type)
    {
    case PrimitiveType::TriangleList:
        return GL_TRIANGLES;
    case PrimitiveType::TriangleStrip:
        return GL_TRIANGLE_STRIP;
    case PrimitiveType::LineList:
        return GL_LINES;
    case PrimitiveType::LineStrip:
        return GL_LINE_STRIP;
    case PrimitiveType::PointList:
        return GL_POINTS;
    }
    assert(false);
    return NULL;
    // TODO: error handling
}

inline GLenum VertexBufferDataUsage_to_gl(VertexBufferDataUsage type)
{
    switch (type)
    {
    case VertexBufferDataUsage::StaticDraw:
        return GL_STATIC_DRAW;
    case VertexBufferDataUsage::DynamicDraw:
        return GL_DYNAMIC_DRAW;
    case VertexBufferDataUsage::StreamDraw:
        return GL_STREAM_DRAW;
    }
    assert(false);
    return NULL;
    // TODO: error handling
}

inline GLenum ImageFormat_to_gl(ImageFormat format)
{
    switch (format)
    {
    case ImageFormat::R8:
        return GL_RED;
    case ImageFormat::Rg16:
        return GL_RG;
    case ImageFormat::Rgb24:
        return GL_RGB;
    case ImageFormat::Rgba32:
        return GL_RGBA;
    }
    assert(false);
    return NULL;
}

inline int ImageFormat_get_size(ImageFormat format)
{
    switch (format)
    {
    case ImageFormat::R8:
        return 1;
    case ImageFormat::Rg16:
        return 2;
    case ImageFormat::Rgb24:
        return 3;
    case ImageFormat::Rgba32:
        return 4;
    }
    assert(false);
    return -1;
}

inline const char* gl_get_error_msg(GLenum err)
{
#define make_case(a) case a: return #a
    switch (err)
    {
        make_case(GL_INVALID_ENUM);
        make_case(GL_INVALID_VALUE);
        make_case(GL_INVALID_OPERATION);
        make_case(GL_OUT_OF_MEMORY);
        make_case(GL_INVALID_FRAMEBUFFER_OPERATION);
    default: return "UNKNOWN_ERROR";
    }
#undef make_case
}

#endif