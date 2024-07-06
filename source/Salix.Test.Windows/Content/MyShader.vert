#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec2 aTex;
out vec4 vColor;
out vec2 vTex;

uniform mat3x2 trans2d;
uniform mat3x2 proj2d;

void main()
{
    vColor = aColor;
    vTex = aTex;

    gl_Position = vec4(proj2d * vec3(trans2d * vec3(aPos, 1.0), 1.0), 0.0, 1.0);
}