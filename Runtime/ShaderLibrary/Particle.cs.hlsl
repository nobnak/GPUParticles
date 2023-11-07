//
// This file was automatically generated. Please don't edit by hand. Execute Editor command [ Edit > Rendering > Generate Shader Includes ] instead
//

#ifndef PARTICLE_CS_HLSL
#define PARTICLE_CS_HLSL
// Generated from GPUParticleSystem.Particle
// PackingRules = Exact
struct Particle
{
    float3 position; // x: x y: y z: z 
    float3 velocity; // x: x y: y z: z 
    float lifetime;
    float duration;
    float size;
    int activity;
};

//
// Accessors for GPUParticleSystem.Particle
//
float3 GetPosition(Particle value)
{
    return value.position;
}
float3 GetVelocity(Particle value)
{
    return value.velocity;
}
float GetLifetime(Particle value)
{
    return value.lifetime;
}
float GetDuration(Particle value)
{
    return value.duration;
}
float GetSize(Particle value)
{
    return value.size;
}
int GetActivity(Particle value)
{
    return value.activity;
}

#endif
