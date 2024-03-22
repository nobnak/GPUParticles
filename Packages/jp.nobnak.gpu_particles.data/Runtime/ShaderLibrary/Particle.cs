using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUParticleSystem.Data {

    [GenerateHLSL(needAccessors = false)]
    public struct Particle {
        public int activity;
        public float3 position;
        public float4 color;

        // life: remaining life, life span, 0, 0
        public float4 life;
    }
}
