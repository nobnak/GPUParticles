using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUParticleSystem {

    [GenerateHLSL(needAccessors = false)]
    public struct Particle {
        public float4 color;

        public float3 position;
        public float lifetime;

        public float3 velocity;
        public float duration;

        public float3 uvw;
        public float size;

        public int activity;
    }
}
