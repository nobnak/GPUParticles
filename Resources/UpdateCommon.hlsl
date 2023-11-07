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
#include "../Runtime/ShaderLibrary/Particle.cs.hlsl"
RWStructuredBuffer<Particle> _Particles;
RWStructuredBuffer<Particle> _ParticlesAdd;
int _OperationMode;

// Time
float _DeltaTime;


#endif