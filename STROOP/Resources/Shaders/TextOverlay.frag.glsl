#version 330 core

in vec2 fs_texCoord;
layout(location=0) out vec4 color;

uniform sampler2D sampler;

void main()
{
	// GDI is funny and actually swaps the red and blue channel - memesters
	color = texture(sampler, fs_texCoord).bgra;
}