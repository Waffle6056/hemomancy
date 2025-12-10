shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	
	vec2 centered_uv = (UV-vec2(.5,.5));
	float center_dis = length(centered_uv);
	float dis_factor = center_dis*center_dis*4.0;
	COLOR = vec4(vec3(1.0-dis_factor),1.0);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
