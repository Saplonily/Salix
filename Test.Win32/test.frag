#version 330 core
out vec4 FragColor;
in vec4 vColor;
in vec2 vTex;

uniform sampler2D tex0;

void main()
{
    FragColor = texture(tex0, vTex) * vColor;
} 