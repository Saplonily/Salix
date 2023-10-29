#version 330 core
out vec4 FragColor;
in vec4 vColor;
in vec2 vTex;

uniform sampler2D tex;
uniform bool isDrawingText;

void main()
{
    FragColor = isDrawingText ? (vec4(1.0, 1.0, 1.0, texture(tex, vTex).g) * vColor) : (texture(tex, vTex) * vColor);
}