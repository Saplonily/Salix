#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec2 aTex;
out vec4 vColor;
out vec2 vTex;

uniform mat4 projectionMat;

void main()
{
    vColor = aColor;
    vTex = aTex;
    gl_Position = projectionMat * vec4(aPos, 1.0);
}