#version 330 core
out vec4 FragColor;
in vec4 vColor;
in vec2 vTex;

uniform sampler2D tex;
uniform bool isDrawingText;

const float offset = 1.0 / 300.0;

void main()
{
    vec2 offsets[9] = vec2[](
        vec2(-offset,  offset),
        vec2( 0.0f,    offset),
        vec2( offset,  offset),
        vec2(-offset,  0.0f),  
        vec2( 0.0f,    0.0f),  
        vec2( offset,  0.0f),  
        vec2(-offset, -offset),
        vec2( 0.0f,   -offset),
        vec2( offset, -offset) 
    );

    float kernel[9] = float[](
        0.0947416, 0.118318, 0.0947416,
        0.1183180, 0.147761, 0.1183180,
        0.0947416, 0.118318, 0.0947416
    );

    vec4 sampleTex[9];
    for (int i = 0; i < 9; i++)
        sampleTex[i] = isDrawingText ?
          (vec4(1.0, 1.0, 1.0, texture(tex, vTex + offsets[i]).g) * vColor) 
        : (texture(tex, vTex + offsets[i]) * vColor);

    vec4 col = vec4(0.0);
    for (int i = 0; i < 9; i++)
        col += sampleTex[i] * kernel[i];

    FragColor = col;
}