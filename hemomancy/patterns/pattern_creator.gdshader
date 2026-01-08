shader_type canvas_item;

uniform vec2 points[16];
const float FALLOFF_RATE = 16.0;

void vertex() {
	// Called for every vertex the material is visible on.
}

float line_distance(vec3 x0, vec3 x1, vec3 x2){
	if (x1 == x2)
		return length(x1-x0);
	return length(cross(x0-x1,x0-x2))/length(x2-x1);
}

float line_distance(vec2 x0, vec2 x1, vec2 x2){
	return line_distance(vec3(x0,0),vec3(x1,0),vec3(x2,0));
}

float closed_line_distance(vec2 x0, vec2 x1, vec2 x2, vec2 x3){
	vec2 line_center = (x1+x2)/2.0;
	vec2 perp = cross(vec3(x1-x2,0),vec3(0,0,1)).xy;
	float dis_from_center_line = line_distance(x0,line_center,line_center+perp);
	if (dis_from_center_line > length(x1-x2)/2.0)
		return max(line_distance(x0,x1,x2),line_distance(x0,x2,x3));
	else
		return line_distance(x0,x1,x2);
}

bool above_line(vec2 x0, vec2 x1, vec2 x2){
	vec2 line_center = (x1+x2)/2.0;
	vec2 perp = cross(vec3(x1-x2,0),vec3(0,0,1)).xy;
	return dot(perp, x0-line_center) > 0.0;
}

float angle(vec2 a, vec2 b){
	return acos(dot(normalize(a),normalize(b)));
}

float step_value(vec2 x0, vec2 x1, vec2 x2){
	return sqrt(1.0+pow(1.0/tan(angle(x1-x0,x2-x1)),2));
}

void fragment() {
	// Called for every pixel the material is visible on.
	int len = 0;
	for (; len < points.length(); len++)
		if (points[len] == vec2(-1))
			break;
	
	
	int line_ind = -1;
	for (int i = 0; i < len-1; i++){
		if (above_line(UV, points[i], points[i+1]) && (line_ind == i-1 || line_ind == -1) && points[i] != points[i+1])
			line_ind = i;
	}
	if (above_line(UV, points[len-1], points[0]) && (line_ind == len-2 || line_ind == -1) && points[len-1] != points[0])
		line_ind = len-1;
		
		
	if (line_ind == -1){
		float min_dis = 1.0;
		for (int i = 0; i < len-1; i++)
			min_dis = min(min_dis,line_distance(UV,points[i],points[i+1]));
		
		min_dis = min(min_dis,line_distance(UV,points[len-1],points[0]));
		float inheritance = 1.0 - min_dis * FALLOFF_RATE;
		COLOR = vec4(0,0,inheritance,1);
	}
	else{
		float min_dis = 1.0;
		if (length(points[(line_ind+1)%len]-UV) > length( points[line_ind]-UV)){
			int ind = (len+line_ind-1)%len;
			min_dis = closed_line_distance(UV, points[(ind+2)%len], points[(ind+1)%len], points[ind]);
		} else {
			min_dis = closed_line_distance(UV, points[line_ind], points[(line_ind+1)%len], points[(line_ind+2)%len]);
		}
		float inheritance = 1.0 - min_dis * FALLOFF_RATE;
	
		vec2 x0 = points[(line_ind+0)%len];
		vec2 x1 = points[(line_ind+1)%len];
		vec2 x2 = points[(line_ind+2)%len];
		vec2 dir = normalize(x1-x0) * .5 + .5;
		float inverse_step = 1.0/step_value(x0,x1,x2);
		COLOR = vec4(dir,inheritance,1);
		//COLOR = vec4(0,0,inheritance,1);
		//COLOR = vec4(inverse_step,0,0,1);
		//COLOR = vec4(dir,0,1);
		//COLOR = vec4(min_dis,0,0,1.0);
	}
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
