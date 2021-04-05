﻿#version 330 core

const vec2 positions[4] = vec2[](
	vec2(-1.0, -1.0),
	vec2( 1.0, -1.0),
	vec2( 1.0,  1.0),
	vec2(-1.0,  1.0)
);

layout (location = 0) in mat4 vs_instanceTransform;
layout (location = 4) in vec4 vs_instanceColor;
layout (location = 5) in vec4 vs_instanceOutlineColor;

uniform mat4 viewProjection;

out vec2 fs_texCoord;
out vec4 fs_color;
out vec4 fs_outline_color;

void main()
{
    gl_Position = (viewProjection * vs_instanceTransform) * vec4(positions[gl_VertexID], 0, 1);
	fs_texCoord = positions[gl_VertexID];
	fs_color = vs_instanceColor;
	fs_outline_color = vs_instanceOutlineColor;
} 