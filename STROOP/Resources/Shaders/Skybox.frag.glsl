#version 330 core

in vec2 fs_screen;

uniform sampler2D sky;
uniform bool useTexture;
uniform vec3 color;
uniform vec2 windowOrigin;

layout(location=0) out vec4 outColor;

void main()
{
	if (!useTexture)
	{
		outColor = vec4(color, 1);
		return;
	}

	float x = windowOrigin.x + fs_screen.x * 320.0;
	float y = windowOrigin.y - fs_screen.y * 240.0;
	outColor = vec4(texture(sky, vec2(x / 1600.0, (960.0 - y) / 960.0)).rgb, 1);
}
