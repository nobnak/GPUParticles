//
// This file was automatically generated. Please don't edit by hand. Execute Editor command [ Edit > Rendering > Generate Shader Includes ] instead
//

#ifndef PARTICLE_CS_HLSL
#define PARTICLE_CS_HLSL
// Generated from Particle
// PackingRules = Exact
struct Particle
{
    float3 position; // x: x y: y z: z 
    float3 velocity; // x: x y: y z: z 
    float duration;
    int activity;
};

//
// Accessors for Particle
//
float3 GetPosition(Particle value)
{
    return value.position;
}
float3 GetVelocity(Particle value)
{
    return value.velocity;
}
float GetDuration(Particle value)
{
    return value.duration;
}
int GetActivity(Particle value)
{
    return value.activity;
}

#endif
