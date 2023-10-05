#version 330 core
out vec4 FragColor;
in vec4 vColor;
in vec2 vTex;

uniform sampler2D tex;

void main()
{
    vec4 texc = texture(tex, vTex);
    FragColor = vec4(1.0, 1.0, 1.0, texc.g) * vColor;
}