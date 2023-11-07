using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUParticleSystem {

    [GenerateHLSL]
    public struct Particle {
        public float3 position;
        public float3 velocity;
        public float lifetime;
        public float duration;
        public float size;
        public int activity;
    }
}
