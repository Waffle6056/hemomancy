shader_type canvas_item;

uniform vec2 points[16];
uniform float border_max_dis = .0625;
uniform int frame_active = 1;

void vertex() {
	// Called for every vertex the material is visible on.
}

vec2 norm_uv(vec2 x){
	return (x+1.0)/2.0;
}

float line_distance(vec3 x0, vec3 x1, vec3 x2){
	if (x1 == x2)
		return length(x1-x0);
	return length(cross(x0-x1,x0-x2))/length(x2-x1);
}

float line_distance(vec2 x0, vec2 x1, vec2 x2){
	return line_distance(vec3(x0,0),vec3(x1,0),vec3(x2,0));
}
vec2 perp(vec2 vec){
	return normalize(cross(vec3(vec,0),vec3(0,0,1)).xy);
}
vec2 displacement(vec2 x0, vec2 x1, vec2 x2){
	return line_distance(x0,x1,x2) * perp(x1-x2) / border_max_dis;
}

vec2 max_displacement(vec2 x0, vec2 x1, vec2 x2, vec2 x3){
	vec2 dis1 = displacement(x0,x1,x2);
	vec2 dis2 = displacement(x0,x2,x3);
	if (length(dis1) > length(dis2))
		return dis1;
	else
		return dis2;
}

vec2 closed_line_displacement(vec2 x0, vec2 x1, vec2 x2, vec2 x3, vec2 x4){
	vec2 line_center = (x2+x3)/2.0;
	vec2 perp = perp(x2-x3);
	float dis_from_center_line = line_distance(x0,line_center,line_center+perp);
	if (dis_from_center_line > length(x2-x3)/2.0){
		if (length(x0-x2) < length(x0-x3))
			return max_displacement(x0,x1,x2,x3);
		else
			return max_displacement(x0,x2,x3,x4);
	}
	else
		return displacement(x0,x2,x3);
}

bool above_line(vec2 x0, vec2 x1, vec2 x2){
	vec2 line_center = (x1+x2)/2.0;
	return dot(perp(x1-x2), x0-line_center) > 0.0;
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
		vec2 min_dis = vec2(1,1);
		for (int i = 0; i < len-1; i++){
			vec2 displacement = displacement(UV,points[i],points[i+1]);
			if (length(displacement) < length(min_dis))
				min_dis = displacement;
		}
		vec2 de = displacement(UV,points[len-1],points[0]);
		if (length(de) < length(min_dis))
			min_dis = de;
		switch (frame_active){
			case 1:
				COLOR = vec4(.5,.5,1,1);
				break;
			case 2:
				COLOR = vec4(norm_uv(min_dis),border_max_dis,1);
				break;
				
		}
	}
	else{
		int ind = (len+line_ind-1)%len;
		vec2 max_dis = closed_line_displacement(UV, points[ind], points[(ind+1)%len], points[(ind+2)%len], points[(ind+3)%len]);
	
		vec2 x0 = points[(line_ind+0)%len];
		vec2 x1 = points[(line_ind+1)%len];
		vec2 x2 = points[(line_ind+2)%len];
		vec2 dir = norm_uv(normalize(x1-x0));
		float inverse_step = 1.0/step_value(x0,x1,x2);
		
		switch (frame_active){
			case 1:
				COLOR = vec4(dir,inverse_step,1);
				break;
			case 2:
				COLOR = vec4(norm_uv(-max_dis),border_max_dis,1);
				break;
				
		}
	
		//if (length(max_dis) <= 1.0)
			//COLOR = vec4(0,0,0,1);
		//COLOR = vec4(inverse_step,0,0,1);
		//COLOR = vec4(dir,0,1);d
		//COLOR = vec4(min_dis,0,0,1.0);
	}
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
