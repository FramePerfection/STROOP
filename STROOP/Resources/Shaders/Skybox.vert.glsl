#version 330 core

out vec2 fs_screen;

void main()
{
	vec2 ndc = vec2((gl_VertexID & 1) * 4.0 - 1.0, (gl_VertexID >> 1) * 4.0 - 1.0);
	gl_Position = vec4(ndc, 1, 1);
	fs_screen = vec2(ndc.x * 0.5 + 0.5, 0.5 - ndc.y * 0.5);
}
