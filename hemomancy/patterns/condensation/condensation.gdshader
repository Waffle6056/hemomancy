shader_type canvas_item;
uniform sampler2D transparency_map;
uniform float map_factor = 2.0;
const float step_size = 64.0/512.0;
uniform int frame_active = 1;

void vertex() {
	// Called for every vertex the material is visible on.
}
float dis_factor(vec2 uv){
	vec2 centered_uv = (uv-vec2(.5,.5));
	float center_dis = length(centered_uv);
	float dis_factor = 1.0-center_dis*center_dis;
	return dis_factor;
}

vec2 vec_to_uv(vec2 vec){
	return (vec + 1.0) / 2.0;
}

void fragment() {
	// Called for every pixel the material is visible o
	vec2 center_vec = vec2(.5,.5)-UV;
	vec2 up_uv = UV+vec2(0,step_size);
	float up_gradient = texture(transparency_map, up_uv).r-texture(transparency_map,UV).r;
	vec2 right_uv = UV+vec2(step_size,0);
	float right_gradient = texture(transparency_map, right_uv).r-texture(transparency_map,UV).r;
	vec2 gradient = vec2(up_gradient,right_gradient);
	vec2 dir = center_vec - gradient * map_factor * length(center_vec);
	//float alpha = max_dis(UV) - map_factor + transparency_offset ;
	switch (frame_active){
		case 1:
			COLOR = vec4(.5,.5,1,1);
			break;
		case 2:
			COLOR = vec4(vec_to_uv(dir), 1, 1);
			break;
	}
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
