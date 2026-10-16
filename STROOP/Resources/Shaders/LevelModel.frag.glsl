#version 330 core

in vec2 fs_texCoord;
in vec4 fs_color;
in float fs_worldY;
in float fs_fog;

uniform sampler2D tex;
uniform bool useTexture;
uniform bool decal;
uniform float alphaCutoff;
uniform bool opaque;
uniform float maxY;
uniform vec3 fogColor;
uniform bool recolorRed;

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
	if (recolorRed && c.r > 0.15 && c.r > 1.5 * c.g && c.r > 1.5 * c.b)
		c.rg = c.gr;
	c.rgb = mix(c.rgb, fogColor, fs_fog);
	if (opaque)
		c.a = 1;
	color = c;
}
