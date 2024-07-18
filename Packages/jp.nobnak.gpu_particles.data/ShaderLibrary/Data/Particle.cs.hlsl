//
// This file was automatically generated. Please don't edit by hand. Execute Editor command [ Edit > Rendering > Generate Shader Includes ] instead
//

#ifndef PARTICLE_CS_HLSL
#define PARTICLE_CS_HLSL
//
// GPUParticleSystem.Data.Activity:  static fields
//
#define ACTIVITY_INACTIVE (0)
#define ACTIVITY_ACTIVE (1)
#define ACTIVITY_DEAD (-1)

// Generated from GPUParticleSystem.Data.Particle
// PackingRules = Exact
struct Particle
{
    int activity;
    float3 position; // x: x y: y z: z 
    float4 color; // x: x y: y z: z w: w 
    float4 life; // x: x y: y z: z w: w 
};


#endif
