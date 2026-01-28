shader_type canvas_item;
render_mode unshaded;

uniform float rotate_time_scale;
uniform float dis_exponent;
uniform vec4 color : source_color = vec4(1.0);
uniform sampler2D transparency_pattern;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {

	// Called for every pixel the material is visible on.
	vec2 centered_uv = UV - .5;
	float dis = abs(length(centered_uv)-.25);
	if (length(centered_uv) < .25){
		//dis = max(0,dis-.2);
		dis /= 4.0;
	}
	float dis_factor = pow(dis,dis_exponent);
	float transparency = max(0.0,1.0-(1.0/.25)*dis_factor);

    float angle = atan(centered_uv.y, centered_uv.x);
	float rotate_time = rotate_time_scale * TIME;
	vec2 rotated_center_uv = vec2(cos(rotate_time+angle),sin(rotate_time+angle))*length(centered_uv);
	vec2 rotated_uv = (rotated_center_uv + vec2(.5f,.5f));
	transparency *= texture(transparency_pattern, rotated_uv).r;
	COLOR = vec4(color.rgb,transparency);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
