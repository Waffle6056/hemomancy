shader_type canvas_item;

uniform sampler2D transparency_pattern;
uniform vec3 color_offset = vec3(.7,0,0);
varying vec2 world_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX,0, 1.0)).xy;
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec2 centered_uv = (UV-vec2(.5,.5));
	float center_dis = length(centered_uv);
	float dis_factor = center_dis*center_dis*4.0;
	vec2 world_uv = mod(world_position / 200.0,1);
	float alpha = (1.0-dis_factor) * texture(transparency_pattern, world_uv).r;
	COLOR.a = min(COLOR.a,alpha);
	COLOR.rgb /= 7.0;
	COLOR.rgb += color_offset;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
