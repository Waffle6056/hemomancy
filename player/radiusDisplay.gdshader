shader_type canvas_item;
uniform float outerOuterRadius = 1.0;
uniform float outerInnerRadius = 0.975;
uniform float outerTimeScale = 0.3;
uniform float outerDotLength = 0.3;
uniform float outerGapLength = 0.1;

void vertex() {
	// Called for every vertex the material is visible on.
}

float angle(vec2 v){
	return acos(dot(normalize(v),vec2(1,0)));
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec2 centered_vec = UV*2.0-1.0;
	if (length(centered_vec) < outerOuterRadius && length(centered_vec) > outerInnerRadius){
		float interval = mod(angle(centered_vec)+TIME*outerTimeScale, (outerGapLength+outerDotLength));
		if (interval <= outerGapLength)
			COLOR.a = 0.0;
		else
			COLOR.a = 1.0;
		
	}
	else
		COLOR.a = 0.0;

	
	
	
	
	
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
