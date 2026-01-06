shader_type canvas_item;
uniform sampler2D transparency_map;
uniform float transparency_offset = -.2;
const float step_size = 64.0/512.0;

void vertex() {
	// Called for every vertex the material is visible on.
}
float max_dis(vec2 uv){
	vec2 centered_uv = (uv-vec2(.5,.5));
	float center_dis = length(centered_uv);
	float dis_factor = 1.0-center_dis*center_dis;
	return dis_factor;
}

void fragment() {
	// Called for every pixel the material is visible o
	vec2 center_vec = normalize(UV-vec2(.5,.5));
	vec2 aft_uv = UV+center_vec*step_size;
	float map_factor = texture(transparency_map, UV).r*(max_dis(UV)-max_dis(aft_uv));
	float alpha = max_dis(UV) - map_factor + transparency_offset ;

	COLOR = vec4(0,0,alpha, 1);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
