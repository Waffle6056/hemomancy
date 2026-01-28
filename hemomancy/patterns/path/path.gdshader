shader_type canvas_item;
uniform sampler2D transparency_map;
const float step_size = 32.0/512.0;
uniform int frame_active = 1;
uniform float y_clip = .1;
uniform float x_scale = .1;
uniform float x_pow = .1;
uniform float y_scale = .1;

void vertex() {
	// Called for every vertex the material is visible on.
}

vec2 vec_to_uv(vec2 vec){
	return (vec + 1.0) / 2.0;
}
vec2 perp(vec2 vec){
	return normalize(cross(vec3(vec,0),vec3(0,0,1)).xy);
}
vec2 squish_vec(vec2 pos){
	//float y_
	//float displacement_from_x = ()*pow(abs(pos.x),x_pow)*x_scale;
	//float y_offset_magnitude = .25*normalize(pos.y);
	//vec2 offset = vec2(0, y_offset_magnitude) * displacement_from_x;
	//vec2 squished_vec = pos * vec2(1,displacement_from_x);
	//return squished_vec;
	return vec2(0);
}

void fragment() {
	// Called for every pixel the material is visible o
	vec2 pos = UV-.5;
	vec2 squished_vec = squish_vec(pos);
	float strength = texture(transparency_map, vec_to_uv(vec2(squished_vec.y,0))).r;


	vec2 pos2 = (UV-vec2(step_size,0))-.5;
	vec2 squished_vec2 = squish_vec(pos2);

	vec2 dir = normalize(squished_vec - squished_vec2) * vec2(1,-1) * strength;
	dir += -perp(dir) * normalize(pos.y) * (0.5-strength);
	if (abs(squished_vec.y) > y_clip)
		dir = vec2(0);

	//vec2 center_vec = vec2(0,.5)-UV;
	//float ang =
	//vec2 up_uv = UV+vec2(0,step_size);
	//float up_gradient = texture(transparency_map, up_uv).r-texture(transparency_map,UV).r;
	//vec2 right_uv = UV+vec2(step_size,0);
	//float right_gradient = texture(transparency_map, right_uv).r-texture(transparency_map,UV).r;
	//vec2 gradient = vec2(up_gradient,right_gradient);
	//vec2 dir = center_vec; //- gradient * map_factor * length(center_vec);
	//float alpha = max_dis(UV) - map_factor + transparency_offset ;
	switch (frame_active){
		case 1:
			COLOR = vec4(vec_to_uv(vec2(0,0)),1,1);
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
