using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUParticleSystem.Data {

    [GenerateHLSL(
        sourcePath: "Packages/jp.nobnak.gpu_particles.data/ShaderLibrary/Data/Particle.cs",
        needAccessors: false)]
    public enum Activity {
        Inactive = 0,
        Active = 1,
        Dead = -1,
    }

    [GenerateHLSL(
        sourcePath: "Packages/jp.nobnak.gpu_particles.data/ShaderLibrary/Data/Particle.cs", 
        needAccessors: false)]
    public struct Particle {
        public Activity activity;
        public float3 position;
        public float4 color;

        // life: remaining life, life span, 0, 0
        public float4 life;
    }
}
