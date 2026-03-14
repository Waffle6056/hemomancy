shader_type canvas_item;
uniform int frame_active = 1;
uniform float radius = 0.1;

void vertex() {
	// Called for every vertex the material is visible on.
}

vec2 vec_to_uv(vec2 vec){
	return (vec + 1.0) / 2.0;
}

void fragment() {
	// Called for every pixel the material is visible o
	float inheritance = 0.0;
	if (length(UV-.5) < radius)
		inheritance = 1.0;
	switch (frame_active){
		case 1:
			COLOR = vec4(.5,.5,1.0,1.0);
			break;
		case 2:
			COLOR = vec4(.5,.5,inheritance,1.0);
			break;
	}
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
