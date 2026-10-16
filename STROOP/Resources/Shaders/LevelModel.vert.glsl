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
uniform mat3 uvTransform = mat3(1.0);
uniform int texGen;
uniform int fogEnabled;
uniform vec2 fogFactors;
uniform vec2 fogPlanes;

out vec2 fs_texCoord;
out vec4 fs_color;
out float fs_worldY;
out float fs_fog;

void main()
{
	vec4 world = model * vec4(vs_position, 1);
	gl_Position = viewProjection * world;
	vec2 uv = vs_texCoord;
	if (texGen != 0)
	{
		vec3 n = viewRotation * (mat3(model) * vs_normal);
		vec2 d = length(n) > 0.0 ? n.xy / length(n) : vec2(0.0);
		uv *= texGen == 2 ? acos(clamp(-d, -1.0, 1.0)) / 3.14159265 : (d + 1.0) * 0.5;
	}
	fs_texCoord = (uvTransform * vec3(uv, 1)).xy;
	fs_worldY = world.y;

	fs_fog = 0.0;
	if (fogEnabled != 0)
	{
		float n = fogPlanes.x, f = fogPlanes.y, d = max(gl_Position.w, n);
		float z = (f + n) / (f - n) - 2.0 * f * n / ((f - n) * d);
		fs_fog = clamp((z * fogFactors.x + fogFactors.y) / 256.0, 0.0, 1.0);
	}

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
