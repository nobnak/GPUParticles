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

        public float lifespan;
        public float life;
    }
}
