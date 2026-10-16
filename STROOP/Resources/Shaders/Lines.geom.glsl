#version 330 core

uniform vec2 viewportSize;

in float gs_width[];
in vec4 gs_color[];

out vec4 fs_color;

layout (lines) in;
layout (triangle_strip, max_vertices = 4) out;

void main()
{
	vec4 c0 = gl_in[0].gl_Position, c1 = gl_in[1].gl_Position;
	vec4 color0 = gs_color[0], color1 = gs_color[1];
	float width0 = gs_width[0], width1 = gs_width[1];

	float d0 = c0.z + c0.w, d1 = c1.z + c1.w;
	if (d0 < 0 && d1 < 0)
		return;
	if (d0 < 0)
	{
		float t = d0 / (d0 - d1);
		c0 = mix(c0, c1, t);
		color0 = mix(color0, color1, t);
		width0 = mix(width0, width1, t);
	}
	else if (d1 < 0)
	{
		float t = d1 / (d1 - d0);
		c1 = mix(c1, c0, t);
		color1 = mix(color1, color0, t);
		width1 = mix(width1, width0, t);
	}

	float w0 = max(c0.w, 1e-6);
	float w1 = max(c1.w, 1e-6);
	vec3 p0 = c0.xyz / w0;
	vec3 p1 = c1.xyz / w1;
	vec2 dist = p1.xy - p0.xy;
	vec2 perpendicular = normalize(vec2(dist.y, -dist.x)) / viewportSize;

	gl_Position = vec4(vec3(p0.xy + perpendicular * width0, p0.z) * w0, w0);
	fs_color = color0;
	EmitVertex();

	gl_Position = vec4(vec3(p0.xy - perpendicular * width0, p0.z) * w0, w0);
	fs_color = color0;
	EmitVertex();

	gl_Position = vec4(vec3(p1.xy + perpendicular * width1, p1.z) * w1, w1);
	fs_color = color1;
	EmitVertex();

	gl_Position = vec4(vec3(p1.xy - perpendicular * width1, p1.z) * w1, w1);
	fs_color = color1;
	EmitVertex();
	EndPrimitive();
}
