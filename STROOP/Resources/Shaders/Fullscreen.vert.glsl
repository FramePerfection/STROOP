#version 330 core

const vec2 positions[3] = vec2[](
	vec2(-1, 1),
	vec2(-1, -3),
	vec2(3,  1)
);

out vec2 fs_texCoord;

void main()
{
    gl_Position = vec4(positions[gl_VertexID], 0, 1);
	fs_texCoord = (positions[gl_VertexID] + vec2(1)) * 0.5;
	fs_texCoord.y = 1 - fs_texCoord.y;
}