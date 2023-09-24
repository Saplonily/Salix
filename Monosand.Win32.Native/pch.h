#pragma once
#ifndef H_PCH
#define H_PCH

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <assert.h>
#include "thirdparty/glad/glad.h"
#include "thirdparty/stb_image.h"

using byte = unsigned char;
#define GL_CHECK_ERROR {\
GLenum err = glGetError();\
if (err != 0)\
printf("ERROR %d (%s, line%d)\n", err, __FUNCDNAME__, __LINE__);\
}\

#endif