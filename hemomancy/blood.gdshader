shader_type canvas_item;
render_mode world_vertex_coords;

uniform sampler2D transparency_map;
uniform float transparency_offset = -.2;
uniform float pulse_magnitude = .1;
uniform float red_range = .3;
uniform float red_offset = .2;
varying vec2 world_position;
varying mat4 transform;

float rand(vec2 co)
{
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

void vertex() {
	world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
	transform = MODEL_MATRIX;
	//COLOR = VERTEX/10.0;
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec2 forward_vec = normalize(transform[0].rg);
	float angle = acos(dot(forward_vec, vec2(1,0)));
	float red_factor = abs(angle/PI)*red_range+red_offset;
	
	vec2 centered_uv = (UV-vec2(.5,.5))*2.0;
	float center_dis = length(centered_uv);
	float dis_factor = sqrt(1.0-center_dis*center_dis);
	vec2 world_uv = mod(world_position / 200.0,1);
	float map_factor = texture(transparency_map, world_uv).r;
	float pulse_factor = pulse_magnitude*(texture(transparency_map, world_uv).r-.5)*sin(TIME);
	float alpha = dis_factor * map_factor + transparency_offset + pulse_factor;
	
	COLOR = vec4(red_factor,0,0, alpha);

}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
