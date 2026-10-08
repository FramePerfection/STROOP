#version 330 core

in vec2 fs_texCoord;
in vec4 fs_color;
in float fs_worldY;

uniform sampler2D tex;
uniform bool useTexture;
uniform bool decal;
uniform float alphaCutoff;
uniform bool opaque;
uniform float maxY;

layout(location=0) out vec4 color;

void main()
{
	if (fs_worldY > maxY)
		discard;

	vec4 c = fs_color;
	if (useTexture)
	{
		vec4 t = texture(tex, fs_texCoord);
		c = decal ? vec4(mix(fs_color.rgb, t.rgb, t.a), fs_color.a) : t * fs_color;
	}

	if (c.a < alphaCutoff)
		discard;
	if (opaque)
		c.a = 1;
	color = c;
}
