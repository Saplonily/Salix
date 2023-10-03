#pragma once
#ifndef H_EXPORTS_GRAPHICS
#define H_EXPORTS_GRAPHICS
#include "pch.h"
#include "enums.h"

struct Color { BYTE r, g, b, a; };

class vertex_type_handle
{
public:
    VertexElementType* type_ptr;
    int length;
    // for the default 'DrawPrimitive' method
    GLuint default_vao_id;
};

class buffer_handle
{
public:
    GLuint vbo_id;
    GLuint vao_id;
    GLuint ibo_id;
};

#endif