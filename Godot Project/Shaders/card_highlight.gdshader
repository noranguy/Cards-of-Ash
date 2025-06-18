shader_type canvas_item;

uniform vec4 glow_color : source_color = vec4(1.0, 1.0, 0.0, 1.0);
uniform float glow_strength = 1.5;
uniform float glow_radius = 0.02;

void fragment() {
	vec2 uv = UV;
	float dist_to_edge = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
	float glow = smoothstep(glow_radius, 0.0, dist_to_edge) * glow_strength;

	vec4 tex_color = texture(TEXTURE, uv);
	COLOR = mix(glow_color, tex_color, 1.0 - glow);
}