#version 330 core

layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec4 aColor;

uniform mat4 uProjection;
uniform mat4 uTransform;

out vec2 vTexCoord;
out vec4 vColor;

void main()
{
    gl_Position = uProjection * uTransform * vec4(aPos, 0.0, 1.0);
    vTexCoord = aTexCoord;
    vColor = aColor;
}
