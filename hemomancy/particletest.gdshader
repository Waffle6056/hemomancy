shader_type particles;
uniform vec3 particle_positions;

void reset(out vec4 MISC, out vec3 VELO, out vec4 FORCE, out mat4 TRANS){
	MISC = vec4(0);
	VELO = vec3(100,0,0);
	FORCE = vec4(0);
	TRANS = mat4(
				vec4(1,0,0,0),
				vec4(0,1,0,0),
				vec4(0,0,1,0),
				vec4(0,0,0,1)
				);
}

void start() {
	// Called when a particle is spawned.
	
	if (USERDATA1.x > 0.0){
		VELOCITY = vec3(-100,0,0);
		USERDATA1.x = 0.0;
	}
	else
		reset(USERDATA1, VELOCITY, USERDATA2, TRANSFORM);
	
	USERDATA2 = vec4(0);
	
}

void process() {
	// Called every frame on existing particles (according to the Fixed FPS property).
	
	if (length(USERDATA2) > 0.0)
		USERDATA1.x = 1.0;
	VELOCITY += USERDATA2.xyz * DELTA;
}
