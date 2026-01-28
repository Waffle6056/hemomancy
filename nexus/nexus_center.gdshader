shader_type canvas_item;

uniform float time_scale;
uniform float pulse_time_scale;
uniform float rotate_time_scale;
uniform sampler2D transparency_pattern;

void vertex() {
	// Called for every vertex the material is visible on.
}



void fragment() {
	// Called for every pixel the material is visible on.
	float time = TIME * time_scale;
	vec2 centered_uv = (UV-vec2(.5,.5));
	float center_dis = length(centered_uv);
	float dis_factor = center_dis*center_dis*4.0 * ((sin(time)+3.5)/2.5);
    float angle = atan(centered_uv.y, centered_uv.x);
	float rotate_time = rotate_time_scale * time;
	vec2 rotated_center_uv = vec2(cos(rotate_time+angle),sin(rotate_time+angle))*center_dis;
	vec2 rotated_uv = (rotated_center_uv + vec2(.5f,.5f));
	vec2 pulse_offset = vec2(cos(angle),sin(angle))*sqrt(center_dis*200.0)/20.0;
	vec2 rotated_uv_slid = mod(rotated_uv+pulse_offset, vec2(1.0,1.0));
	float alpha = (1.0-dis_factor) * texture(transparency_pattern, rotated_uv_slid).r;
	COLOR = vec4(dis_factor,0,0,alpha);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
