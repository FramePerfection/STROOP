#version 330 core

layout (location = 0) in vec3 vs_position;
layout (location = 1) in vec2 vs_texCoord;
layout (location = 2) in vec4 vs_color;
layout (location = 3) in vec4 vs_ambient;
layout (location = 4) in vec3 vs_normal;
layout (location = 5) in vec3 vs_lightDir;

uniform mat4 viewProjection;
uniform mat4 model;
uniform mat3 viewRotation;

out vec2 fs_texCoord;
out vec4 fs_color;
out float fs_worldY;

void main()
{
	vec4 world = model * vec4(vs_position, 1);
	gl_Position = viewProjection * world;
	fs_texCoord = vs_texCoord;
	fs_worldY = world.y;

	vec4 color = vs_color;
	if (vs_ambient.a > 0.5)
	{
		vec3 n = viewRotation * (mat3(model) * vs_normal);
		float nLength = length(n), lLength = length(vs_lightDir);
		float k = nLength > 0.0 && lLength > 0.0 ? max(0.0, dot(n / nLength, vs_lightDir / lLength)) : 0.0;
		color.rgb = min(vec3(1.0), vs_ambient.rgb + vs_color.rgb * k);
	}
	fs_color = color;
}
