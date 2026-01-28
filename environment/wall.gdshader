shader_type canvas_item;
uniform vec2 brick_size;
uniform sampler2D brick_pattern;
uniform float edge_percent = 0.05;
varying vec2 world_position;

void vertex()
{
    world_position = (MODEL_MATRIX * vec4(VERTEX,0, 1.0)).xy + vec2(1000,1000);
}

void fragment() {
	
	NORMAL = vec3(0,0,1);
	// Called for every pixel the material is visible on.
	vec2 pos = world_position + texture(brick_pattern,mod(world_position.xy/brick_size/vec2(2000.0,500.0),1.0)).xy*brick_size;
	if (mod(pos.y,brick_size.y*2.0) <= brick_size.y)
		pos.x += brick_size.x/2.0;
	vec4 brick_outline = texture(TEXTURE,mod(pos/brick_size,1.0));
	
	brick_outline = (brick_outline/vec4(vec3(30),1.0)+vec4(vec3(.2),1.0));
	
	vec2 brick_uv = mod(pos,brick_size)/brick_size;
	
	
	
	if (min(brick_uv.x,brick_uv.y) <= edge_percent || max(brick_uv.x,brick_uv.y) >= 1.0-edge_percent){
		float factor = min(min(brick_uv.x,brick_uv.y),1.0-max(brick_uv.x,brick_uv.y));
		brick_outline = vec4(edge_percent-factor)/edge_percent/4.0 + vec4(.25);
		vec2 dir = normalize(brick_uv-vec2(.5));
		NORMAL = vec3(dir-dir*cos(factor/edge_percent*PI/2.0),1.0-sin(factor/edge_percent*PI/2.0));
	}
	
	NORMAL += (texture(brick_pattern,mod(pos/brick_size/vec2(200.0,50.0),1.0)).rgb-vec3(.5))*.1;
	
	vec4 brick_color = texture(brick_pattern,mod(pos/brick_size/vec2(200.0,50.0),1.0)) / 2.0 + vec4(.7);
	
	//COLOR = mix(brick_color, brick_outline/vec4(vec3(5.0),1.0),0.2);
	COLOR = brick_outline * brick_color;
	COLOR.a = 1.0;
	COLOR.rgb += vec3(.225,.225,.2);
	NORMAL = NORMAL.xzy;
	
	//COLOR = vec4(NORMAL+vec3(.5),1.0);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
