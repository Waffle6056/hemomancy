shader_type canvas_item;
uniform sampler2D orig;
uniform float b_offset = 0.1;
void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	//UV -= vec2(0,.5);
	vec2 center = vec2(.5,0);
	//vec2 center2 = vec2(.5,.8);
	vec2 center_vec = UV-center;
	//if (acos(dot(normalize(center_vec),vec2(0,1))) < PI/32.0 || length(center2-UV) < .2)
		//COLOR = vec4(0,0,1.0-length(center_vec),1);
	//else
		//COLOR = vec4(0,0,0,1);
	COLOR = texture(orig,UV);
	if (COLOR.b > 0.0){
		//COLOR.b *= 1.0-length(center_vec);
		//COLOR.b += b_offset;
	}
	COLOR = vec4(0,0,COLOR.b,1);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
