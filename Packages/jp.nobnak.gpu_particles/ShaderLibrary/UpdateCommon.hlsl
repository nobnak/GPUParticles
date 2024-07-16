#ifndef __UPDATE_COMMON_HLSLINC__
#define __UPDATE_COMMON_HLSLINC__


// Counter
uint _ThreadCount;
StructuredBuffer<uint> _CounterBuffer;

// Index Pool
AppendStructuredBuffer<uint> _IndexPoolA;
ConsumeStructuredBuffer<uint> _IndexPoolC;

// Active IDs
AppendStructuredBuffer<uint> _ActiveIndexesA;

// Particles
#include "Packages/jp.nobnak.gpu_particles.data/Runtime/ShaderLibrary/Particle.cs.hlsl"
RWStructuredBuffer<Particle> _Particles;
RWStructuredBuffer<Particle> _ParticlesAdd;


#endif