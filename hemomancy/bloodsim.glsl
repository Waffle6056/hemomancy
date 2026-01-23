#[compute]
#version 450

layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;


layout(set = 0,binding = 0) restrict buffer EnemyPosition{
	vec2 position[];
} enemy_position;
layout(set = 0,binding = 1) restrict buffer EnemyRadius{
	float radius[];
} enemy_radius;
layout(set = 0,binding = 2) coherent restrict buffer EnemyDamage{
    int damage[];//modified by all
} enemy_damage;



layout(set = 0,binding = 3) restrict buffer ParticlePosition{
	vec2 position[];//persistent
} particle_position;

layout(set = 0,binding = 4) restrict buffer ParticleVelocity{
	vec2 velocity[];//persistent
} particle_velocity;

layout(set = 0,binding = 5) restrict buffer ParticleInUse{
	bool in_use[];//persistent
} particle_in_use;

layout(set = 0,binding = 6) restrict buffer ParticleToInstantiate{
	int count_to_instantiate;//modified by all
	int count_per_position[];
} particle_to_instantiate;

layout(set = 0,binding = 7, std140) restrict buffer ParticleInstantiatePosition{
	vec2 position[];
} particle_instantiate_position;

layout(set = 0,binding = 8) restrict buffer ParticleMisc{
	float delta_time;
	float mass[];//persistent
} particle_misc;



layout(set = 0,binding = 9) restrict buffer PlayerFieldTransform{
	mat3x2 field_transform[];
} player_field_transform;

layout(set = 0,binding = 10) restrict buffer PlayerFieldVelocityRot{
	vec4 field_velocity_rot[];
} player_field_velocity_rot;

layout(set = 0,binding = 11) restrict buffer PlayerFieldMagnitude{
	vec2 field_magnitude[];
} player_field_magnitude;

layout(set = 0,binding = 12) restrict buffer PlayerFieldPattern{
    int pattern_index[];
} player_field_pattern;


layout(binding = 13) uniform sampler2D patterns[16];
layout(binding = 14) uniform sampler2D patterns2[16]; 

layout(binding = 15) restrict buffer MultiMeshBuffer{
	mat2x4[] transforms;
} multi_mesh_transforms;

float FRICTION_COEFFICIENT = 3.0;
float BORDER_WIDTH = 50;
float GRADIENT_STEP_SIZE = 1.0/256.0;
float MAX_MASS = 1.5;
float MIN_MASS = .5;

float rand(vec2 co)
{
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}
float rand(float a){
	return rand(vec2(a));
}

vec2 scale(mat3x2 mat){
	return vec2(length(mat[0]),length(mat[1]));
}

float angle_to(vec2 a, vec2 b){
	return acos(dot(normalize(a),normalize(b)));
}

vec2 perp(vec2 a){
	return (cross(vec3(a,0),vec3(0,0,1))).xy;
}

vec2 uv_to_vec(vec2 a){
	return a*2.0-1.0;
}

mat2 rotate2d(float angle) {
    return mat2(
        cos(-angle), -sin(-angle),
        sin(-angle),  cos(-angle)
    );
}

float calc_scaled_line_displacement(mat3x2 field_transform, vec2 vecA){
	vec2 size = scale(field_transform);
	vec2 vecB = perp(vecA);
	float angle = angle_to(vecA*size, vecB*size);
	float cur_line_displacement = sin(min(angle,3.14-angle)) * length(vecA*size);
	return cur_line_displacement;
}

vec2 calc_scaled_acceleration(mat3x2 field_transform, vec2 vecA){
	return normalize(-perp(scale(field_transform) * perp(vecA)));// * calc_scaled_line_displacement(field_transform,vecA);
}

void instantiate_particle(uint particle_ind, int count_left){
	int i = 0;
	while (i < particle_to_instantiate.count_per_position.length()){
		if (count_left > particle_to_instantiate.count_per_position[i])
			count_left -= particle_to_instantiate.count_per_position[i];
		else
			break;
	}
	particle_in_use.in_use[particle_ind] = true;
	particle_position.position[particle_ind] = particle_instantiate_position.position[i];
	particle_velocity.velocity[particle_ind] = rotate2d(rand(particle_ind*.1)*2*3.14) * vec2(rand(particle_ind*.1+1),0) * 300;
	particle_misc.mass[particle_ind] = rand(vec2(particle_ind*.1+2));
	particle_misc.mass[particle_ind] = MIN_MASS + particle_misc.mass[particle_ind] * (MAX_MASS-MIN_MASS);
}

float solve_for_time(vec2 v, vec2 a, float x){
	if (length(v) != 0)
		return x/length(v);
	return 999;
	//return (-v+sqrt(v*v+2*a*x))/a;
}

void main() {

    uint particle_ind = gl_GlobalInvocationID.x;
	multi_mesh_transforms.transforms[particle_ind] = mat2x4(0);
	if (particle_in_use.in_use[particle_ind] == false){ // instantiates this invocation as a new particle if not in use and new particles are queued
		int count_left = atomicAdd(particle_to_instantiate.count_to_instantiate,-1);
		if (count_left > 0){
			instantiate_particle(particle_ind, count_left);
		}
		else
			return;
	}


	
	vec2 particle_pos = particle_position.position[particle_ind];
	float min_line_displacement=1.0/0.0;
	int field_ind = -1;
	vec2 uv = vec2(0);
	mat3 to_local;
	bool in_a_field = false;
	for (int i = 0; i < player_field_transform.field_transform.length(); i++){
		mat3 inv = inverse(mat3(player_field_transform.field_transform[i]));
		vec2 local_pos = (inv * vec3(particle_pos,1)).xy + .5;
		

		if (min(local_pos.x,local_pos.y) >= 0 && max(local_pos.x,local_pos.y) <= 1){
			vec4 cur_pattern_point2 = texture(patterns2[player_field_pattern.pattern_index[i]], local_pos);
			vec2 vecA = uv_to_vec(cur_pattern_point2.rg) * cur_pattern_point2.b;
			float cur_line_displacement = calc_scaled_line_displacement(player_field_transform.field_transform[i], vecA);
			if (cur_line_displacement < min_line_displacement){
				min_line_displacement = cur_line_displacement;
				field_ind = i;
				uv = local_pos;
				to_local = inv;
				in_a_field = true;
			}
		}
	}
	float inheritance_value;
	float total_time_used = 0;
	vec2 velocity = particle_velocity.velocity[particle_ind];
	
	do {

		vec2 acceleration = vec2(0);
		float distance_step = BORDER_WIDTH;
		if (in_a_field && min(uv.x,uv.y) >= 0 && max(uv.x,uv.y) <= 1){
		
			int pattern_ind = player_field_pattern.pattern_index[field_ind];
			vec4 pattern_point = texture(patterns[pattern_ind], uv);
			vec4 pattern_point2 = texture(patterns2[pattern_ind], uv);
			mat3x2 field_transform = player_field_transform.field_transform[field_ind];

//			distance_step = 1.0/pattern_point.b * BORDER_WIDTH;
			
			vec2 vecA = uv_to_vec(pattern_point2.rg) * pattern_point2.b;
			float cur_line_displacement = calc_scaled_line_displacement(field_transform, vecA);
			float max_pixel_length = calc_scaled_line_displacement(field_transform, normalize(vecA)*pattern_point2.b);
			inheritance_value = max(0.0,min(1.0, (max_pixel_length - cur_line_displacement)/(max_pixel_length-BORDER_WIDTH)));

			if (inheritance_value >= 1.0 && length(uv_to_vec(pattern_point.rg) * player_field_magnitude.field_magnitude[field_ind].x) > 0){
				velocity = normalize(mat2(field_transform) * uv_to_vec(pattern_point.rg)) * player_field_magnitude.field_magnitude[field_ind].x;
			}
			else {
				// float gradient_x = texture(patterns[pattern_ind], vec2(min(1.0,uv.x+GRADIENT_STEP_SIZE),uv.y)).b - texture(patterns[pattern_ind], vec2(max(0.0,uv.x-GRADIENT_STEP_SIZE),uv.y)).b;
				// float gradient_y = texture(patterns[pattern_ind], vec2(uv.x,min(1.0,uv.y+GRADIENT_STEP_SIZE))).b - texture(patterns[pattern_ind], vec2(uv.x,max(0.0,uv.y-GRADIENT_STEP_SIZE))).b;
				// if (!(gradient_y == 0 && gradient_x == 0))
				acceleration = normalize(mat2(field_transform) * calc_scaled_acceleration(field_transform,vecA)) / particle_misc.mass[particle_ind] * inheritance_value * player_field_magnitude.field_magnitude[field_ind].y;
			}
		}
		float time_used = solve_for_time(velocity, acceleration,distance_step);
		time_used = min(time_used,particle_misc.delta_time-total_time_used);
		
		total_time_used += time_used;

		particle_pos += velocity * time_used + .5 * acceleration * time_used * time_used;
		velocity += acceleration * time_used - velocity * min(1.0,FRICTION_COEFFICIENT * particle_misc.mass[particle_ind] * time_used);


		uv = (to_local * vec3(particle_pos,1)).xy + .5;
	} while (total_time_used < particle_misc.delta_time);

	if (in_a_field){
		vec2 momentary_velocity = vec2(0);
		momentary_velocity += player_field_velocity_rot.field_velocity_rot[field_ind].rg * inheritance_value;
		vec2 field_origin = vec2(player_field_transform.field_transform[field_ind][2][0],player_field_transform.field_transform[field_ind][2][1]);
		vec2 field_particle_vector = particle_pos - field_origin;
		momentary_velocity += (rotate2d(player_field_velocity_rot.field_velocity_rot[field_ind].z * inheritance_value * particle_misc.delta_time) * field_particle_vector - field_particle_vector) / particle_misc.delta_time; 	
		particle_pos += momentary_velocity * particle_misc.delta_time;
	}

	vec2 xaxis = vec2(sqrt(particle_misc.mass[particle_ind]),0);
	xaxis = rotate2d((particle_misc.mass[particle_ind]-MIN_MASS)/(MAX_MASS-MIN_MASS) * 3.14) * xaxis;
	vec2 yaxis = rotate2d(3.14/2) * xaxis;
	
	multi_mesh_transforms.transforms[particle_ind][0][0] =  xaxis.x;
	multi_mesh_transforms.transforms[particle_ind][0][1] =  xaxis.y;
	multi_mesh_transforms.transforms[particle_ind][1][0] =  yaxis.x;
	multi_mesh_transforms.transforms[particle_ind][1][1] =	yaxis.y;
	multi_mesh_transforms.transforms[particle_ind][0][3] =  particle_pos.x;
	multi_mesh_transforms.transforms[particle_ind][1][3] =  particle_pos.y;

	particle_position.position[particle_ind] = particle_pos;
    particle_velocity.velocity[particle_ind] = velocity;

	for (int i = 0; i < enemy_position.position.length(); i++){
		if (length(enemy_position.position[i]-particle_pos) <= enemy_radius.radius[i]){ //TODO line circle collision
			atomicAdd(enemy_damage.damage[i],1);
			particle_in_use.in_use[particle_ind] = false;
		}
	}

		
}