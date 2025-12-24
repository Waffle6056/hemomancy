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



layout(set = 0,binding = 9) restrict buffer PlayerFieldPosition{
	vec2 field_position[];
} player_field_position;

layout(set = 0,binding = 10) restrict buffer PlayerFieldVelocity{
	vec2 field_velocity[];
} player_field_velocity;

layout(set = 0,binding = 11) restrict buffer PlayerFieldMagnitude{
	float field_magnitude[];
} player_field_magnitude;

layout(set = 0,binding = 12) restrict buffer PlayerFieldPattern{
    int pattern_index[];
} player_field_pattern;

layout(set = 0,binding = 13) restrict buffer PlayerFieldSize{
    float field_size[];
} player_field_size;


layout(binding = 14) uniform sampler2D patterns[16];//vector2, arbitrary value defining whether the vector is force or velocity, inherited velocity value
float FORCE_VECTOR = 1.0;
float VELOCITY_VECTOR = 0.0;
float FRICTION_COEFFICIENT = 10.0;

float rand(vec2 co)
{
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

void main() {

    uint particle_ind = gl_GlobalInvocationID.x;
	if (particle_in_use.in_use[particle_ind] == false){ // instantiates this invocation as a new particle if not in use and new particles are queued
		int count_left = atomicAdd(particle_to_instantiate.count_to_instantiate,-1);
		if (count_left > 0){
			int i = 0;
			while (i < particle_to_instantiate.count_per_position.length()){
				if (count_left > particle_to_instantiate.count_per_position[i])
					count_left -= particle_to_instantiate.count_per_position[i];
				else
					break;
			}
			particle_in_use.in_use[particle_ind] = true;
			particle_position.position[particle_ind] = particle_instantiate_position.position[i];
			particle_velocity.velocity[particle_ind] = (vec2(rand(vec2(1,particle_ind)),rand(vec2(particle_ind,1)))-.5) * 100;
			particle_misc.mass[particle_ind] = rand(vec2(particle_ind));
		}
		else
			return;
	}


	
	vec2 particle_pos = particle_position.position[particle_ind];
	float max_inheritance_value = 0;
	vec4 pattern_point = vec4(0);
	int field_ind = -1;
	for (int i = 0; i < player_field_position.field_position.length(); i++){
		vec2 uv = (particle_pos - player_field_position.field_position[i] + player_field_size.field_size[i]) /  player_field_size.field_size[i];
		if (min(uv.x,uv.y) >= 0 && max(uv.x,uv.y) <= 1){
			vec4 cur_pattern_point = texture(patterns[player_field_pattern.pattern_index[i]], uv);
			if (pattern_point.a > max_inheritance_value){
				pattern_point = cur_pattern_point;
				field_ind = i;
			}
		}

	}
	if (field_ind != -1){
		if (pattern_point.b == VELOCITY_VECTOR)
			particle_velocity.velocity[particle_ind] = pattern_point.xy * player_field_magnitude.field_magnitude[field_ind];
		else if (pattern_point.b == FORCE_VECTOR)
			particle_velocity.velocity[particle_ind] += pattern_point.xy / particle_misc.mass[particle_ind] * player_field_magnitude.field_magnitude[field_ind] * particle_misc.delta_time;;
	
		particle_velocity.velocity[particle_ind] += player_field_velocity.field_velocity[field_ind] * max_inheritance_value;
	}
	particle_position.position[particle_ind] = particle_pos += particle_velocity.velocity[particle_ind] * particle_misc.delta_time;

	if (field_ind != -1)
		particle_velocity.velocity[particle_ind] -= player_field_velocity.field_velocity[field_ind] * max_inheritance_value;
	particle_velocity.velocity[particle_ind] -= FRICTION_COEFFICIENT * particle_misc.mass[particle_ind] * particle_misc.delta_time;
	
	for (int i = 0; i < enemy_position.position.length(); i++){
		if (length(enemy_position.position[i]-particle_pos) <= enemy_radius.radius[i]){ //TODO line circle collision
			atomicAdd(enemy_damage.damage[i],1);
			//particle_in_use.in_use[particle_ind] = false;
		}
	}
	
}