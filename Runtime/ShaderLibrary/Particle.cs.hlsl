//
// This file was automatically generated. Please don't edit by hand. Execute Editor command [ Edit > Rendering > Generate Shader Includes ] instead
//

#ifndef PARTICLE_CS_HLSL
#define PARTICLE_CS_HLSL
// Generated from GPUParticleSystem.Particle
// PackingRules = Exact
struct Particle
{
    float4 color; // x: x y: y z: z w: w 
    float3 position; // x: x y: y z: z 
    float lifetime;
    float3 velocity; // x: x y: y z: z 
    float duration;
    float3 uvw; // x: x y: y z: z 
    float size;
    int activity;
};


#endif
