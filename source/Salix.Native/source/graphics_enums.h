#pragma once
#ifndef H_ENUMS_GRAPHICS
#define H_ENUMS_GRAPHICS

#include <assert.h>
#include <glad/glad.h>

// ../Salix/Graphics/Vertex/VertexElementType.cs
enum class VertexElementType
{
    Single,
    Color,
    Vector2,
    Vector3,
    Vector4,
};

// ../Salix/Graphics/Vertex/PrimitiveType.cs
enum class PrimitiveType
{
    TriangleList,
    TriangleStrip,
    TriangleFan,
    LineList,
    LineStrip,
    PointList
};

// ../Salix/Graphics/Vertex/VertexBufferDataUsage.cs
enum class VertexBufferDataUsage
{
    StaticDraw,
    DynamicDraw,
    StreamDraw
};

// ../Salix/Graphics/ImageFormat.cs
enum class ImageFormat
{
    R8,
    Rg16,
    Rgb24,
    Rgba32
};

// ../Salix/Graphics/TextureFilterType.cs
enum class TextureFilterType
{
    Linear,
    Nearest,
    LinearMipmapLinear,
    LinearMipmapNearest,
    NearestMipmapLinear,
    NearestMipmapNearest
};

// ../Salix/Graphics/TextureWrapType.cs
enum class TextureWrapType
{
    ClampToEdge,
    Repeat,
    MirroredRepeat
};

struct vertex_element_glinfo { int count; GLenum type; GLsizei componentSize; };
inline vertex_element_glinfo VertexElementType_get_glinfo(VertexElementType type)
{
    switch (type)
    {
    case VertexElementType::Color: return vertex_element_glinfo{ 4, GL_FLOAT, sizeof(float) };
    case VertexElementType::Single: return vertex_element_glinfo{ 1, GL_FLOAT, sizeof(float) };
    case VertexElementType::Vector2: return vertex_element_glinfo{ 2, GL_FLOAT, sizeof(float) };
    case VertexElementType::Vector3: return vertex_element_glinfo{ 3, GL_FLOAT, sizeof(float) };
    case VertexElementType::Vector4: return vertex_element_glinfo{ 4, GL_FLOAT, sizeof(float) };
    }
    assert(false);
    return vertex_element_glinfo{ 0, 0, 0 };
}

inline GLenum PrimitiveType_get_glinfo(PrimitiveType type)
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
    assert(false);
    return -1;
}

inline GLenum VertexBufferDataUsage_to_gl(VertexBufferDataUsage type)
{
    switch (type)
    {
    case VertexBufferDataUsage::StreamDraw: return GL_STREAM_DRAW;
    case VertexBufferDataUsage::StaticDraw: return GL_STATIC_DRAW;
    case VertexBufferDataUsage::DynamicDraw: return GL_DYNAMIC_DRAW;
    }
    assert(false);
    return -1;
}

inline GLenum ImageFormat_to_gl(ImageFormat format)
{
    switch (format)
    {
    case ImageFormat::R8: return GL_RED;
    case ImageFormat::Rg16: return GL_RG;
    case ImageFormat::Rgb24: return GL_RGB;
    case ImageFormat::Rgba32: return GL_RGBA;
    }
    assert(false);
    return -1;
}

inline int ImageFormat_get_size(ImageFormat format)
{
    switch (format)
    {
    case ImageFormat::R8: return 1;
    case ImageFormat::Rg16: return 2;
    case ImageFormat::Rgb24: return 3;
    case ImageFormat::Rgba32: return 4;
    }
    assert(false);
    return -1;
}

inline GLenum TextureFilterType_to_gl(TextureFilterType type)
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
    assert(false);
    return -1;
}

inline GLenum TextureWrapType_to_gl(TextureWrapType type)
{
    switch (type)
    {
    case TextureWrapType::ClampToEdge: return GL_CLAMP_TO_EDGE;
    case TextureWrapType::Repeat: return GL_REPEAT;
    case TextureWrapType::MirroredRepeat: return GL_MIRRORED_REPEAT;
    }
    assert(false);
    return -1;
}

#endif